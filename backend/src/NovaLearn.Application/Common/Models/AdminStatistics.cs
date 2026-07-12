namespace NovaLearn.Application.Common.Models;

/// <summary>
/// Real, database-backed facts used to compose the admin dashboard. Everything here is
/// computed from live data (users, roles, sessions); the query handler layers the
/// not-yet-modelled domains (courses, finance, etc.) on top as server-provided values.
/// </summary>
public sealed record AdminStatistics(
    int TotalUsers,
    int ActiveUserCount,
    int VerifiedUsers,
    int UnverifiedUsers,
    int NewUsersLast30Days,
    int NewUsersPrev30Days,
    int ActiveSessions,
    int FailedLoginAttempts,
    int PublishedCourses,
    int DraftCourses,
    IReadOnlyList<RoleCount> RoleCounts,
    IReadOnlyList<MonthlyCount> MonthlyRegistrations,
    IReadOnlyList<AdminUserBrief> RecentUsers,
    IReadOnlyList<AdminUserBrief> RecentUnverifiedUsers);

/// <summary>Number of members assigned to a role.</summary>
public sealed record RoleCount(string Role, int Count);

/// <summary>Registration count for a single calendar month.</summary>
public sealed record MonthlyCount(int Year, int Month, int Count);

/// <summary>A lightweight user projection for feeds and approval queues.</summary>
public sealed record AdminUserBrief(
    Guid Id,
    string FullName,
    string Email,
    DateTimeOffset CreatedAtUtc,
    bool EmailConfirmed);
