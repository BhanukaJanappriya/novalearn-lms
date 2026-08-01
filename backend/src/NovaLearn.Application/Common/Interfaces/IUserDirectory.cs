using NovaLearn.Application.Common.Models;
using NovaLearn.Shared.Common;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>
/// Read-side port for account administration. Lives apart from
/// <see cref="IUserAdministration"/> because listing is a database query while the mutations
/// have to go through ASP.NET Identity.
/// </summary>
public interface IUserDirectory
{
    /// <summary>Paged, filtered account search. All filters are optional and combine with AND.</summary>
    Task<PagedResult<AdminUserRow>> SearchAsync(
        string? search,
        string? role,
        bool? isActive,
        bool? emailConfirmed,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>A single account, or null when it does not exist (or is soft-deleted).</summary>
    Task<AdminUserRow?> GetAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// How many accounts currently hold a role. Used to stop the last super administrator
    /// being demoted or deactivated, which would lock everyone out of the platform.
    /// </summary>
    Task<int> CountInRoleAsync(string role, CancellationToken cancellationToken);
}
