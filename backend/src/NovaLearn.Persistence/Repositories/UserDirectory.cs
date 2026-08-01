using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Common;

namespace NovaLearn.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the account directory. The soft-delete filter on Users applies
/// automatically, so deleted accounts never appear.
///
/// Roles are loaded as a second query keyed by the ids on the current page rather than as a
/// correlated collection subquery inside the projection. The identity join table has no
/// navigation property, and a per-row subquery of role names is the kind of construct that
/// either fails to translate or degrades into N+1. Two flat queries are predictable and, for
/// an admin-only page of at most 100 rows, cheap.
/// </summary>
internal sealed class UserDirectory(ApplicationDbContext context) : IUserDirectory
{
    public async Task<PagedResult<AdminUserRow>> SearchAsync(
        string? search,
        string? role,
        bool? isActive,
        bool? emailConfirmed,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        int safePage = page < 1 ? 1 : page;
        int safePageSize = pageSize < 1 ? 20 : pageSize;

        IQueryable<ApplicationUser> query = context.Users;

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search.Trim()}%";
            query = query.Where(u =>
                EF.Functions.ILike(u.FirstName, pattern)
                || EF.Functions.ILike(u.LastName, pattern)
                || EF.Functions.ILike(u.Email!, pattern));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            // Materialised first so the filter becomes a plain parameterised IN clause.
            List<Guid> holders = await UserIdsInRoleQuery(role).ToListAsync(cancellationToken);
            query = query.Where(u => holders.Contains(u.Id));
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        if (emailConfirmed.HasValue)
        {
            query = query.Where(u => u.EmailConfirmed == emailConfirmed.Value);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<UserFacts> facts = await ProjectFacts(
                query
                    .OrderByDescending(u => u.CreatedAtUtc)
                    .Skip((safePage - 1) * safePageSize)
                    .Take(safePageSize))
            .ToListAsync(cancellationToken);

        IReadOnlyDictionary<Guid, List<string>> roles =
            await LoadRolesAsync(facts.Select(f => f.Id).ToList(), cancellationToken);

        List<AdminUserRow> rows = facts.Select(f => ToRow(f, roles)).ToList();

        return new PagedResult<AdminUserRow>(rows, safePage, safePageSize, totalCount);
    }

    public async Task<AdminUserRow?> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        UserFacts? facts = await ProjectFacts(context.Users.Where(u => u.Id == userId))
            .FirstOrDefaultAsync(cancellationToken);

        if (facts is null)
        {
            return null;
        }

        IReadOnlyDictionary<Guid, List<string>> roles = await LoadRolesAsync([userId], cancellationToken);
        return ToRow(facts, roles);
    }

    public Task<int> CountInRoleAsync(string role, CancellationToken cancellationToken) =>
        UserIdsInRoleQuery(role).Distinct().CountAsync(cancellationToken);

    /// <summary>Ids of every account holding a role. Composed, never called inside a projection.</summary>
    private IQueryable<Guid> UserIdsInRoleQuery(string role) =>
        from userRole in context.UserRoles
        join appRole in context.Roles on userRole.RoleId equals appRole.Id
        where appRole.Name == role
        select userRole.UserId;

    /// <summary>
    /// Everything about a user that comes from the Users table plus scalar counts. Kept separate
    /// from roles so the projection stays flat and translatable.
    /// </summary>
    private IQueryable<UserFacts> ProjectFacts(IQueryable<ApplicationUser> source)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        return source.Select(u => new UserFacts(
            u.Id,
            u.FirstName,
            u.LastName,
            u.Email ?? string.Empty,
            u.AvatarUrl,
            u.EmailConfirmed,
            u.IsActive,
            u.LockoutEnd != null && u.LockoutEnd > now,
            u.CreatedAtUtc,
            u.LastLoginAtUtc,
            context.Enrollments.Count(e => e.StudentId == u.Id && e.Status != EnrollmentStatus.Dropped),
            context.Courses.Count(c => c.LecturerId == u.Id)));
    }

    private async Task<IReadOnlyDictionary<Guid, List<string>>> LoadRolesAsync(
        IReadOnlyList<Guid> userIds, CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<Guid, List<string>>();
        }

        var pairs = await (
                from userRole in context.UserRoles
                join appRole in context.Roles on userRole.RoleId equals appRole.Id
                where userIds.Contains(userRole.UserId)
                select new { userRole.UserId, appRole.Name })
            .ToListAsync(cancellationToken);

        return pairs
            .GroupBy(p => p.UserId)
            .ToDictionary(g => g.Key, g => g.Select(p => p.Name ?? string.Empty).Order().ToList());
    }

    private static AdminUserRow ToRow(UserFacts f, IReadOnlyDictionary<Guid, List<string>> roles) => new(
        f.Id,
        f.FirstName,
        f.LastName,
        f.Email,
        f.AvatarUrl,
        f.EmailConfirmed,
        f.IsActive,
        f.IsLockedOut,
        f.CreatedAtUtc,
        f.LastLoginAtUtc,
        roles.TryGetValue(f.Id, out List<string>? held) ? held : [],
        f.EnrollmentCount,
        f.CoursesOwned);

    /// <summary>Intermediate projection shape; never leaves this class.</summary>
    private sealed record UserFacts(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        string? AvatarUrl,
        bool EmailConfirmed,
        bool IsActive,
        bool IsLockedOut,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset? LastLoginAtUtc,
        int EnrollmentCount,
        int CoursesOwned);
}
