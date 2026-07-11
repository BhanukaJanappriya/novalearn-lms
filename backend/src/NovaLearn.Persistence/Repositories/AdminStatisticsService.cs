using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the admin read model. All figures are derived from live tables;
/// the soft-delete query filter on <c>Users</c> is respected automatically.
/// </summary>
internal sealed class AdminStatisticsService(ApplicationDbContext context) : IAdminStatisticsService
{
    public async Task<AdminStatistics> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset last30 = now.AddDays(-30);
        DateTimeOffset prev30 = now.AddDays(-60);
        DateTimeOffset windowStart =
            new DateTimeOffset(new DateTime(now.Year, now.Month, 1), TimeSpan.Zero).AddMonths(-11);

        int totalUsers = await context.Users.CountAsync(cancellationToken);
        int verifiedUsers = await context.Users.CountAsync(u => u.EmailConfirmed, cancellationToken);
        int activeUsers = await context.Users.CountAsync(u => u.IsActive, cancellationToken);
        int newLast30 = await context.Users.CountAsync(u => u.CreatedAtUtc >= last30, cancellationToken);
        int newPrev30 = await context.Users
            .CountAsync(u => u.CreatedAtUtc >= prev30 && u.CreatedAtUtc < last30, cancellationToken);
        int failedLogins = await context.Users.SumAsync(u => u.AccessFailedCount, cancellationToken);
        int activeSessions = await context.RefreshTokens
            .CountAsync(rt => rt.RevokedAtUtc == null && rt.ExpiresAtUtc > now, cancellationToken);

        List<RoleCount> roleCounts = await (
                from u in context.Users
                join ur in context.UserRoles on u.Id equals ur.UserId
                join r in context.Roles on ur.RoleId equals r.Id
                group r by r.Name into grouped
                select new RoleCount(grouped.Key!, grouped.Count()))
            .ToListAsync(cancellationToken);

        // Small dataset: pull the created-dates in the 12-month window and bucket in memory,
        // avoiding provider-specific date-part grouping translation.
        List<DateTimeOffset> createdDates = await context.Users
            .Where(u => u.CreatedAtUtc >= windowStart)
            .Select(u => u.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        List<MonthlyCount> monthlyRegistrations = createdDates
            .GroupBy(d => new { d.Year, d.Month })
            .Select(g => new MonthlyCount(g.Key.Year, g.Key.Month, g.Count()))
            .ToList();

        List<AdminUserBrief> recentUsers = await ProjectBriefsAsync(
            context.Users.OrderByDescending(u => u.CreatedAtUtc).Take(8), cancellationToken);

        List<AdminUserBrief> recentUnverified = await ProjectBriefsAsync(
            context.Users.Where(u => !u.EmailConfirmed).OrderByDescending(u => u.CreatedAtUtc).Take(5),
            cancellationToken);

        return new AdminStatistics(
            TotalUsers: totalUsers,
            ActiveUserCount: activeUsers,
            VerifiedUsers: verifiedUsers,
            UnverifiedUsers: totalUsers - verifiedUsers,
            NewUsersLast30Days: newLast30,
            NewUsersPrev30Days: newPrev30,
            ActiveSessions: activeSessions,
            FailedLoginAttempts: failedLogins,
            RoleCounts: roleCounts,
            MonthlyRegistrations: monthlyRegistrations,
            RecentUsers: recentUsers,
            RecentUnverifiedUsers: recentUnverified);
    }

    private static async Task<List<AdminUserBrief>> ProjectBriefsAsync(
        IQueryable<ApplicationUser> query, CancellationToken cancellationToken)
    {
        var rows = await query
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email, u.CreatedAtUtc, u.EmailConfirmed })
            .ToListAsync(cancellationToken);

        return rows
            .Select(u => new AdminUserBrief(
                u.Id,
                $"{u.FirstName} {u.LastName}".Trim(),
                u.Email ?? string.Empty,
                u.CreatedAtUtc,
                u.EmailConfirmed))
            .ToList();
    }
}
