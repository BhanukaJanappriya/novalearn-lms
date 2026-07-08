using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using NovaLearn.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace NovaLearn.API.IntegrationTests;

/// <summary>
/// Boots the real API against a throwaway PostgreSQL container. The schema is created from the EF
/// model (no migrations required), and admin seeding is disabled so tests start from a clean slate.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _database.GetConnectionString(),
                ["Jwt:Issuer"] = "NovaLearnTest",
                ["Jwt:Audience"] = "NovaLearnTestClient",
                ["Jwt:SigningKey"] = "integration-test-signing-key-at-least-32-bytes",
                ["Jwt:AccessTokenLifetimeMinutes"] = "15",
                ["Jwt:RefreshTokenLifetimeDays"] = "7",
                ["App:FrontendBaseUrl"] = "http://localhost:5173",
                ["Seed:SuperAdminPassword"] = string.Empty
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _database.StartAsync();

        using IServiceScope scope = Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _database.DisposeAsync();
        await base.DisposeAsync();
    }
}
