namespace NovaLearn.Domain.Identity;

/// <summary>
/// A persisted, rotating refresh token. Only the SHA-256 hash of the token is stored
/// (<see cref="TokenHash"/>); the raw value exists solely in the client's possession.
/// Rotation is enforced: redeeming a token revokes it and records its replacement, so a
/// replayed (already-rotated) token can be detected and the whole chain revoked.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid UserId { get; private set; }

    public ApplicationUser User { get; private set; } = null!;

    /// <summary>SHA-256 hash of the opaque token value. Never store the raw token.</summary>
    public string TokenHash { get; private set; } = null!;

    /// <summary>Ties the refresh token to the access token (JWT) it was issued alongside.</summary>
    public string JwtId { get; private set; } = null!;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public string? RevokedByIp { get; private set; }

    /// <summary>Hash of the token that replaced this one during rotation (audit trail).</summary>
    public string? ReplacedByTokenHash { get; private set; }

    public string? CreatedByIp { get; private set; }

    public bool IsExpired(DateTimeOffset utcNow) => utcNow >= ExpiresAtUtc;

    public bool IsRevoked => RevokedAtUtc is not null;

    public bool IsActive(DateTimeOffset utcNow) => !IsRevoked && !IsExpired(utcNow);

    private RefreshToken() { } // EF Core

    public static RefreshToken Issue(
        Guid userId,
        string tokenHash,
        string jwtId,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc,
        string? createdByIp)
    {
        return new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            JwtId = jwtId,
            CreatedAtUtc = createdAtUtc,
            ExpiresAtUtc = expiresAtUtc,
            CreatedByIp = createdByIp
        };
    }

    /// <summary>Marks this token as consumed during rotation and links it to its successor.</summary>
    public void Revoke(DateTimeOffset revokedAtUtc, string? revokedByIp, string? replacedByTokenHash = null)
    {
        RevokedAtUtc = revokedAtUtc;
        RevokedByIp = revokedByIp;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
