namespace NovaLearn.API.Features.Admin;

/// <summary>Query string for the account directory. All members are optional.</summary>
public sealed record AdminUserSearchRequest
{
    /// <summary>Case-insensitive match against first name, last name or email.</summary>
    public string? Search { get; init; }

    /// <summary>Exact role name, e.g. "Lecturer".</summary>
    public string? Role { get; init; }

    public bool? IsActive { get; init; }

    public bool? EmailConfirmed { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

/// <summary>Body for enabling or disabling sign-in on an account.</summary>
public sealed record SetUserStatusRequest(bool IsActive);

/// <summary>Body for replacing an account's roles with exactly this set.</summary>
public sealed record UpdateUserRolesRequest(IReadOnlyList<string> Roles);
