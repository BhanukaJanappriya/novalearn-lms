using NovaLearn.Application.Features.Security.Common;
using NovaLearn.Shared.Common;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>Read side of the security center: sessions, lockouts, and the headline figures about them.</summary>
public interface ISecurityRepository
{
    Task<SecurityOverview> GetOverviewAsync(CancellationToken cancellationToken);

    /// <summary>Active sessions (not revoked, not expired), newest first. All filters are optional.</summary>
    Task<PagedResult<SessionRow>> ListActiveSessionsAsync(
        string? search, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>Accounts currently locked out by repeated failed sign-ins, soonest-to-unlock first.</summary>
    Task<PagedResult<LockedAccountRow>> ListLockedAccountsAsync(
        string? search, int page, int pageSize, CancellationToken cancellationToken);
}
