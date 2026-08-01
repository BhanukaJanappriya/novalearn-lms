using NovaLearn.Application.Common.Models;

namespace NovaLearn.Application.UnitTests.Admin;

/// <summary>Small builders so the account-administration tests stay about behaviour, not setup.</summary>
internal static class UserAdminTestData
{
    public static AdminUserRow User(Guid id, params string[] roles) => new(
        Id: id,
        FirstName: "Test",
        LastName: "User",
        Email: "test.user@novalearn.local",
        AvatarUrl: null,
        EmailConfirmed: true,
        IsActive: true,
        IsLockedOut: false,
        CreatedAtUtc: DateTimeOffset.UtcNow.AddDays(-30),
        LastLoginAtUtc: DateTimeOffset.UtcNow.AddDays(-1),
        Roles: roles,
        EnrollmentCount: 0,
        CoursesOwned: 0);
}
