namespace NovaLearn.Application.Common.Models;

/// <summary>
/// One person as the people directory shows them.
///
/// This is deliberately narrower than <see cref="AdminUserRow"/>. That type backs the account
/// administration screen and carries security state: lockout, failed sign-in counts, the
/// activate and deactivate controls. A directory answers "who is on the platform and how are
/// they doing", so it omits all of it. Anyone who needs to act on an account goes to
/// /admin/users, where the authority checks live.
///
/// Nothing here is an academic record either: counts and averages appear, individual grades and
/// submissions do not.
/// </summary>
public sealed record DirectoryEntry(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? AvatarUrl,
    /// <summary>Whether the account can still sign in. A directory that showed departed people
    /// as current would mislead, so this one flag is kept.</summary>
    bool IsActive,
    DateTimeOffset JoinedAtUtc,
    DateTimeOffset? LastActiveAtUtc,
    IReadOnlyList<string> Roles,
    DirectoryLearnerStats? Learner,
    DirectoryTeacherStats? Teacher)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
}

/// <summary>How a learner is doing, in aggregate only.</summary>
public sealed record DirectoryLearnerStats(
    int EnrolledCourses,
    int CompletedCourses,
    int AverageProgressPercent);

/// <summary>What a member of teaching staff is responsible for.</summary>
public sealed record DirectoryTeacherStats(
    int CoursesOwned,
    int PublishedCourses,
    int LearnersTaught,
    IReadOnlyList<string> DepartmentsHeaded);
