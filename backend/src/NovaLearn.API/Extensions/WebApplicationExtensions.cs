using Microsoft.Extensions.DependencyInjection;
using NovaLearn.Persistence.Seeding;

namespace NovaLearn.API.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>Applies migrations and seeds baseline data at startup, in a dedicated DI scope.</summary>
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        ApplicationDbContextInitialiser initialiser =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.MigrateAsync();
        await initialiser.SeedAsync();
    }
}
