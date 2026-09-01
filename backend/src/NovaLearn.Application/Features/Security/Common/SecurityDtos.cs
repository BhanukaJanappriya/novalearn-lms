namespace NovaLearn.Application.Features.Security.Common;

/// <summary>
/// The security center's headline figures. Every number here is real: sessions and lockouts are
/// counted straight off the tables that actually gate sign-in, not a synthetic "posture score" —
/// see the remarks on <c>SecurityPanel</c> (the admin dashboard's older, decorative widget) for
/// what this deliberately does not carry over.
/// </summary>
public sealed record SecurityOverview(
    int ActiveSessions,
    int LockedOutAccounts,
    int FailedLoginAttempts,
    double TwoFactorAdoptionPct);

/// <summary>One active (not revoked, not expired) refresh token, as an administrator sees it.</summary>
public sealed record SessionRow(
    Guid Id,
    Guid UserId,
    string UserName,
    string UserEmail,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    string? CreatedByIp);

/// <summary>One currently locked-out account.</summary>
public sealed record LockedAccountRow(
    Guid UserId,
    string UserName,
    string UserEmail,
    DateTimeOffset LockoutEnd,
    int AccessFailedCount);
