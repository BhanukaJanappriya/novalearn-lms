using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

    private static string Describe(IdentityResult result) =>
        string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
}
