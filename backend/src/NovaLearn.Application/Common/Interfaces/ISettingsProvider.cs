using NovaLearn.Application.Common.Models;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>
/// The read side of platform settings: cheap enough to call from anywhere that needs to consult a
/// setting in passing (registration gate, checkout currency, upload limit, the maintenance-mode
/// check on every request). Backed by a short cache in the implementation, since some of those
/// call sites run on every request and a database round trip per request is not free.
/// </summary>
public interface ISettingsProvider
{
    Task<PlatformSettingsSnapshot> GetAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Drops the cached copy. Called once, right after an edit is saved, so the new values are
    /// visible immediately rather than for however long the cache would otherwise have held the
    /// old ones — the entire point of maintenance mode is that flipping it takes effect at once.
    /// </summary>
    void Invalidate();
}
