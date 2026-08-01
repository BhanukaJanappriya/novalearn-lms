namespace NovaLearn.Application.Common.Models;

/// <summary>
/// An administrative view of one account: identity, state, roles, and enough activity to
/// judge whether the account matters before acting on it.
/// </summary>
public sealed record AdminUserRow(
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
    IReadOnlyList<string> Roles,
    int EnrollmentCount,
    int CoursesOwned)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
}
