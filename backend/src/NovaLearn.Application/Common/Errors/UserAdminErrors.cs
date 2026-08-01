using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Common.Errors;

/// <summary>Central catalogue of account-administration failures.</summary>
public static class UserAdminErrors
{
    public static readonly Error NotFound =
        Error.NotFound("user_admin.not_found", "The requested user was not found.");

    public static readonly Error Unauthenticated =
        Error.Unauthorized("user_admin.unauthenticated", "You must be signed in to administer users.");

    public static readonly Error CannotModifySelf =
        Error.Forbidden("user_admin.cannot_modify_self", "You cannot change your own access. Ask another administrator.");

    public static readonly Error SuperAdminOnly =
        Error.Forbidden(
            "user_admin.super_admin_only",
            "Only a super administrator can manage a super administrator account.");

    public static readonly Error CannotGrantSuperAdmin =
        Error.Forbidden(
            "user_admin.cannot_grant_super_admin",
            "Only a super administrator can grant or revoke the super administrator role.");

    public static readonly Error LastSuperAdmin =
        Error.Conflict(
            "user_admin.last_super_admin",
            "This is the last super administrator. Promote another account first.");

    public static Error UnknownRole(string role) =>
        Error.Validation("user_admin.unknown_role", $"'{role}' is not a known role.");

    public static Error Identity(string description) =>
        Error.Validation("user_admin.identity_error", description);
}
