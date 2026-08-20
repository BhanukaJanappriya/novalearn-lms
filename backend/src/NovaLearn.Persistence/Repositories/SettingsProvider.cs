using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Domain.Settings;

namespace NovaLearn.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the settings read side, backed by an in-process cache.
///
/// Deliberately <see cref="IMemoryCache"/> rather than the distributed cache this project also
/// references: settings are read constantly (potentially every request, via maintenance mode) and
/// written rarely, and this API runs as a single instance in every environment it actually runs
/// in today. A distributed cache would only earn its cost across multiple instances, which would
/// also need <see cref="Invalidate"/> to become a broadcast instead of a local clear — worth
/// doing if a second instance ever exists, not before.
/// </summary>
internal sealed class SettingsProvider(ApplicationDbContext context, IMemoryCache cache) : ISettingsProvider
{
    private const string CacheKey = "platform-settings";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    public async Task<PlatformSettingsSnapshot> GetAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(CacheKey, out PlatformSettingsSnapshot? cached) && cached is not null)
        {
            return cached;
        }

        PlatformSettings settings = await context.PlatformSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException(
                "Platform settings have not been seeded. This should happen automatically on startup.");

        var snapshot = new PlatformSettingsSnapshot(
            settings.SiteName,
            settings.SupportEmail,
            settings.AllowNewRegistrations,
            settings.MaintenanceModeEnabled,
            settings.MaintenanceMessage,
            settings.DefaultCurrency,
            settings.MaxUploadSizeMb);

        cache.Set(CacheKey, snapshot, CacheDuration);

        return snapshot;
    }

    public void Invalidate() => cache.Remove(CacheKey);
}
