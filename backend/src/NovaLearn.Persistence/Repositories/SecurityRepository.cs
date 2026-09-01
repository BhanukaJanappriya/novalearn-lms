using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Security.Common;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Common;

namespace NovaLearn.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the security center's read model. "Active" for a session means not
/// revoked and not expired, computed against the same clock every other session check in the app
/// uses (<see cref="RefreshToken.IsActive"/>), so this list agrees with what would actually still
/// authenticate a request right now.
/// </summary>
internal sealed class SecurityRepository(ApplicationDbContext context) : ISecurityRepository
{
    public async Task<SecurityOverview> GetOverviewAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        int activeSessions = await context.RefreshTokens
            .AsNoTracking()
            .CountAsync(rt => rt.RevokedAtUtc == null && rt.ExpiresAtUtc > now, cancellationToken);

        int lockedOutAccounts = await context.Users
            .AsNoTracking()
            .CountAsync(u => u.LockoutEnd != null && u.LockoutEnd > now, cancellationToken);

        int failedLoginAttempts = await context.Users
            .AsNoTracking()
            .SumAsync(u => u.AccessFailedCount, cancellationToken);

        int totalUsers = await context.Users.AsNoTracking().CountAsync(cancellationToken);
        int twoFactorEnabled = await context.Users
            .AsNoTracking()
            .CountAsync(u => u.TwoFactorEnabled, cancellationToken);

        double twoFactorAdoptionPct =
            totalUsers == 0 ? 0 : Math.Round(twoFactorEnabled / (double)totalUsers * 100, 1);

        return new SecurityOverview(activeSessions, lockedOutAccounts, failedLoginAttempts, twoFactorAdoptionPct);
    }

    public async Task<PagedResult<SessionRow>> ListActiveSessionsAsync(
        string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        int safePage = page < 1 ? 1 : page;
        int safePageSize = pageSize < 1 ? 20 : pageSize;

        IQueryable<RefreshToken> query = context.RefreshTokens
            .AsNoTracking()
            .Include(rt => rt.User)
            .Where(rt => rt.RevokedAtUtc == null && rt.ExpiresAtUtc > now);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search.Trim()}%";
            query = query.Where(rt =>
                EF.Functions.ILike(rt.User.Email!, pattern)
                || EF.Functions.ILike(rt.User.FirstName + " " + rt.User.LastName, pattern));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<RefreshToken> pageItems = await query
            .OrderByDescending(rt => rt.CreatedAtUtc)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        List<SessionRow> rows = pageItems
            .Select(rt => new SessionRow(
                rt.Id,
                rt.UserId,
                $"{rt.User.FirstName} {rt.User.LastName}".Trim(),
                rt.User.Email ?? string.Empty,
                rt.CreatedAtUtc,
                rt.ExpiresAtUtc,
                rt.CreatedByIp))
            .ToList();

        return new PagedResult<SessionRow>(rows, safePage, safePageSize, totalCount);
    }

    public async Task<PagedResult<LockedAccountRow>> ListLockedAccountsAsync(
        string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        int safePage = page < 1 ? 1 : page;
        int safePageSize = pageSize < 1 ? 20 : pageSize;

        IQueryable<ApplicationUser> query = context.Users
            .AsNoTracking()
            .Where(u => u.LockoutEnd != null && u.LockoutEnd > now);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search.Trim()}%";
            query = query.Where(u =>
                EF.Functions.ILike(u.Email!, pattern)
                || EF.Functions.ILike(u.FirstName + " " + u.LastName, pattern));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<ApplicationUser> pageItems = await query
            .OrderBy(u => u.LockoutEnd)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        List<LockedAccountRow> rows = pageItems
            .Select(u => new LockedAccountRow(
                u.Id, $"{u.FirstName} {u.LastName}".Trim(), u.Email ?? string.Empty,
                u.LockoutEnd!.Value, u.AccessFailedCount))
            .ToList();

        return new PagedResult<LockedAccountRow>(rows, safePage, safePageSize, totalCount);
    }
}
