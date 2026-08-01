using NovaLearn.Application.Common.Models;

namespace NovaLearn.Application.Features.Admin.Users.Common;

/// <summary>Read model for the account management table.</summary>
public sealed record AdminUserDto(
    Guid Id,
    string FullName,
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
    public static AdminUserDto FromRow(AdminUserRow row) => new(
        row.Id,
        row.FullName,
        row.FirstName,
        row.LastName,
        row.Email,
        row.AvatarUrl,
        row.EmailConfirmed,
        row.IsActive,
        row.IsLockedOut,
        row.CreatedAtUtc,
        row.LastLoginAtUtc,
        row.Roles,
        row.EnrollmentCount,
        row.CoursesOwned);
}
