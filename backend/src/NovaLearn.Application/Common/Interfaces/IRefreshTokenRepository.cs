using NovaLearn.Domain.Identity;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>Persistence port for <see cref="RefreshToken"/> aggregate access.</summary>
public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken);

    /// <summary>Loads a refresh token (with its <see cref="RefreshToken.User"/>) by token hash.</summary>
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken);

    /// <summary>
    /// Revokes every currently-active refresh token for a user. Used to break the token chain
    /// when replay of a rotated token is detected.
    /// </summary>
    Task RevokeAllActiveForUserAsync(
        Guid userId, DateTimeOffset revokedAtUtc, string? revokedByIp, CancellationToken cancellationToken);
}
