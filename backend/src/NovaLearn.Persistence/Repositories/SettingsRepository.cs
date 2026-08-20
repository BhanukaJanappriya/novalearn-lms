using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Settings;

namespace NovaLearn.Persistence.Repositories;

/// <summary>EF Core implementation of the settings write side: a tracked load of the singleton row.</summary>
internal sealed class SettingsRepository(ApplicationDbContext context) : ISettingsRepository
{
    public async Task<PlatformSettings> GetAsync(CancellationToken cancellationToken) =>
        await context.PlatformSettings.FirstOrDefaultAsync(cancellationToken)
        ?? throw new InvalidOperationException(
            "Platform settings have not been seeded. This should happen automatically on startup.");
}
