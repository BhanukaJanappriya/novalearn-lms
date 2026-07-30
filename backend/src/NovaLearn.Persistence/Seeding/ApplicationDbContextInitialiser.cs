using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Persistence.Seeding;

/// <summary>
/// Applies pending migrations and seeds baseline data (roles + super administrator).
/// Invoked once at startup. All operations are idempotent.
/// </summary>
public sealed class ApplicationDbContextInitialiser(
    ILogger<ApplicationDbContextInitialiser> logger,
    ApplicationDbContext dbContext,
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IOptions<SeedOptions> seedOptions)
{
    private readonly SeedOptions _seed = seedOptions.Value;

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedSuperAdminAsync();
        await SeedEnrollmentsAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (string roleName in Roles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var role = new ApplicationRole(roleName, $"{roleName} role", isSystemRole: true);
            IdentityResult result = await roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                logger.LogInformation("Seeded role {Role}", roleName);
            }
            else
            {
                logger.LogError("Failed to seed role {Role}: {Errors}", roleName, Describe(result));
            }
        }
    }

    private async Task SeedSuperAdminAsync()
    {
        if (string.IsNullOrWhiteSpace(_seed.SuperAdminPassword))
        {
            logger.LogWarning(
                "Seed:SuperAdminPassword is not configured; skipping super administrator seeding.");
            return;
        }

        if (await userManager.FindByEmailAsync(_seed.SuperAdminEmail) is not null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = _seed.SuperAdminEmail,
            Email = _seed.SuperAdminEmail,
            EmailConfirmed = true,
            FirstName = _seed.SuperAdminFirstName,
            LastName = _seed.SuperAdminLastName,
            IsActive = true
        };

        IdentityResult created = await userManager.CreateAsync(admin, _seed.SuperAdminPassword);
        if (!created.Succeeded)
        {
            logger.LogError("Failed to seed super administrator: {Errors}", Describe(created));
            return;
        }

        await userManager.AddToRoleAsync(admin, Roles.SuperAdministrator);
        logger.LogInformation("Seeded super administrator {Email}", _seed.SuperAdminEmail);
    }

    /// <summary>
    /// Seeds a small set of demo enrolments so the catalogue, "my courses" and the admin
    /// dashboard have real data on a fresh database. Runs only while the table is empty, and
    /// fills in demo students / demo courses only if those tables are empty too, so it never
    /// competes with data an operator has already created.
    /// </summary>
    private async Task SeedEnrollmentsAsync()
    {
        if (await dbContext.Enrollments.AnyAsync())
        {
            return;
        }

        List<ApplicationUser> students = await GetOrCreateDemoStudentsAsync();
        if (students.Count == 0)
        {
            logger.LogInformation("No students available; skipping enrollment seeding.");
            return;
        }

        List<Course> courses = await GetOrCreateDemoCoursesAsync();
        if (courses.Count == 0)
        {
            logger.LogInformation("No published courses available; skipping enrollment seeding.");
            return;
        }

        // Deterministic spread: each student joins a rotating window of courses, with staggered
        // enrolment dates across the trailing year and a mix of progress values so the
        // dashboard trend and the status badges have variety.
        DateTimeOffset now = DateTimeOffset.UtcNow;
        int[] progressSteps = [0, 15, 40, 65, 90, 100];
        var seeded = new List<Enrollment>();

        for (int s = 0; s < students.Count; s++)
        {
            int take = 2 + (s % 3);
            for (int c = 0; c < take; c++)
            {
                Course course = courses[(s + c) % courses.Count];
                DateTimeOffset enrolledAt = now.AddDays(-((s * 37) + (c * 11) + 3));

                Enrollment enrollment = Enrollment.Create(students[s].Id, course.Id, enrolledAt);
                int progress = progressSteps[(s + c) % progressSteps.Length];
                if (progress > 0)
                {
                    enrollment.UpdateProgress(progress, enrolledAt.AddDays(20));
                }

                seeded.Add(enrollment);
            }
        }

        await dbContext.Enrollments.AddRangeAsync(seeded);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} demo enrollments", seeded.Count);
    }

    private async Task<List<ApplicationUser>> GetOrCreateDemoStudentsAsync()
    {
        IList<ApplicationUser> existing = await userManager.GetUsersInRoleAsync(Roles.Student);
        if (existing.Count > 0)
        {
            return existing.Take(8).ToList();
        }

        if (string.IsNullOrWhiteSpace(_seed.SuperAdminPassword))
        {
            return [];
        }

        (string First, string Last)[] names =
        [
            ("Ada", "Lovelace"), ("Grace", "Hopper"), ("Alan", "Turing"),
            ("Katherine", "Johnson"), ("Linus", "Pauling"), ("Marie", "Curie"),
        ];

        var created = new List<ApplicationUser>();
        foreach ((string first, string last) in names)
        {
            string email = $"{first}.{last}@novalearn.local".ToLowerInvariant();
            var student = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = first,
                LastName = last,
                IsActive = true
            };

            IdentityResult result = await userManager.CreateAsync(student, _seed.SuperAdminPassword);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to seed demo student {Email}: {Errors}", email, Describe(result));
                continue;
            }

            await userManager.AddToRoleAsync(student, Roles.Student);
            created.Add(student);
        }

        logger.LogInformation("Seeded {Count} demo students (password matches Seed:SuperAdminPassword)", created.Count);
        return created;
    }

    private async Task<List<Course>> GetOrCreateDemoCoursesAsync()
    {
        List<Course> published = await dbContext.Courses
            .Where(c => c.Status == CourseStatus.Published)
            .OrderByDescending(c => c.CreatedAtUtc)
            .Take(8)
            .ToListAsync();

        if (published.Count > 0 || await dbContext.Courses.AnyAsync())
        {
            return published;
        }

        ApplicationUser? owner = await userManager.FindByEmailAsync(_seed.SuperAdminEmail);
        if (owner is null)
        {
            return [];
        }

        (string Title, string Code, string Category, CourseLevel Level, decimal Price)[] demos =
        [
            ("Introduction to Programming", "CS101", "Computer Science", CourseLevel.Beginner, 0m),
            ("Data Structures and Algorithms", "CS201", "Computer Science", CourseLevel.Intermediate, 79m),
            ("Machine Learning Foundations", "AI301", "Artificial Intelligence", CourseLevel.Advanced, 129m),
            ("Financial Accounting I", "BUS110", "Business", CourseLevel.Beginner, 49m),
            ("Digital Marketing Essentials", "MKT120", "Business", CourseLevel.Beginner, 39m),
            ("Organic Chemistry", "CHM240", "Science", CourseLevel.Intermediate, 89m),
        ];

        var courses = demos
            .Select(d => Course.Create(
                d.Title, d.Code,
                $"A demo course seeded so the catalogue has content. Covers the fundamentals of {d.Category.ToLowerInvariant()}.",
                d.Category, d.Level, CourseStatus.Published, d.Price, null, owner.Id))
            .ToList();

        await dbContext.Courses.AddRangeAsync(courses);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} demo courses", courses.Count);
        return courses;
    }

    private static string Describe(IdentityResult result) =>
        string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
}
