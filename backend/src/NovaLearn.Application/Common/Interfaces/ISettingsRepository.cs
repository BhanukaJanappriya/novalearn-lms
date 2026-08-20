using NovaLearn.Domain.Settings;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>
/// The write side of platform settings: a tracked load of the singleton row, for the admin screen
/// that edits it. Separate from <see cref="ISettingsProvider"/>, which is the cheap, cached,
/// read-only port everything else uses — editing needs a fresh, trackable entity, not a cached
/// snapshot.
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// The one settings row. Throws if it has not been seeded, since that indicates a startup bug
    /// rather than an ordinary "not found" a caller should have to handle.
    /// </summary>
    Task<PlatformSettings> GetAsync(CancellationToken cancellationToken);
}
