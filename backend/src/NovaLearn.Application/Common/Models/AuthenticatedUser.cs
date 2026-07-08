namespace NovaLearn.Application.Common.Models;

/// <summary>
/// A framework-agnostic projection of an identity user, returned by <c>IIdentityService</c>.
/// Keeps ASP.NET Identity types out of the Application use cases.
/// </summary>
public sealed record AuthenticatedUser(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool EmailConfirmed,
    IReadOnlyList<string> Roles)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
}
