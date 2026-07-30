using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaLearn.Domain.Content;
using NovaLearn.Domain.Courses;
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
        await SeedCourseContentAsync();
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
    /// Gives every existing course a small demo outline so the course builder has something to
    /// show. Skipped entirely once any module exists, so authored content is never touched.
    /// </summary>
    private async Task SeedCourseContentAsync()
    {
        if (await dbContext.CourseModules.AnyAsync())
        {
            return;
        }

        List<Course> courses = await dbContext.Courses
            .OrderBy(c => c.CreatedAtUtc)
            .ToListAsync();

        if (courses.Count == 0)
        {
            logger.LogInformation("No courses to seed content for; skipping course content seeding.");
            return;
        }

        foreach (Course course in courses)
        {
            AddDemoOutline(course);
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Seeded demo content for {Count} course(s)", courses.Count);
    }

    private void AddDemoOutline(Course course)
    {
        CourseModule welcome = CourseModule.Create(
            course.Id, "Getting started", $"Orientation and setup for {course.Title}.", 0);

        welcome.AddLesson(
            "Welcome and course overview", LessonType.Video,
            "https://videos.novalearn.local/welcome.mp4", null, 6, 0, isPreview: true);
        welcome.AddLesson(
            "How this course is structured", LessonType.Text, null,
            "Each module builds on the last. Work through the lessons in order and take the practice tasks as you go.",
            4, 1, isPreview: true);
        welcome.AddLesson(
            "Syllabus", LessonType.Pdf,
            "https://files.novalearn.local/syllabus.pdf", null, null, 2, isPreview: false);

        CourseModule core = CourseModule.Create(
            course.Id, "Core concepts", "The ideas the rest of the course depends on.", 1);

        core.AddLesson(
            "Key terminology", LessonType.Text, null,
            "A short glossary of the terms used throughout the course.", 10, 0, isPreview: false);
        core.AddLesson(
            "Worked example", LessonType.Video,
            "https://videos.novalearn.local/worked-example.mp4", null, 18, 1, isPreview: false);

        CourseModule practice = CourseModule.Create(
            course.Id, "Practice and next steps", "Apply what you have learned.", 2);

        practice.AddLesson(
            "Practice workbook", LessonType.Pdf,
            "https://files.novalearn.local/workbook.pdf", null, 45, 0, isPreview: false);
        practice.AddLesson(
            "Further reading", LessonType.Link,
            "https://novalearn.local/library", null, null, 1, isPreview: false);

        dbContext.CourseModules.AddRange(welcome, core, practice);
    }

    private static string Describe(IdentityResult result) =>
        string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
}
