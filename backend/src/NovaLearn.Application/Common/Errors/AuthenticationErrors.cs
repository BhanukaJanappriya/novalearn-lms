using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Common.Errors;

/// <summary>
/// Central catalogue of authentication failures. Credential-related errors are deliberately
/// generic to avoid account enumeration.
/// </summary>
public static class AuthenticationErrors
{
    public static readonly Error InvalidCredentials =
        Error.Unauthorized("auth.invalid_credentials", "Invalid email or password.");

    public static readonly Error EmailNotConfirmed =
        Error.Forbidden("auth.email_not_confirmed", "Please verify your email address before signing in.");

    public static readonly Error AccountInactive =
        Error.Forbidden("auth.account_inactive", "This account has been deactivated. Contact an administrator.");

    public static readonly Error AccountLockedOut =
        Error.Forbidden("auth.account_locked_out", "Account locked due to too many failed attempts. Try again later.");

    public static readonly Error EmailAlreadyInUse =
        Error.Conflict("auth.email_in_use", "An account with this email already exists.");

    public static readonly Error InvalidRefreshToken =
        Error.Unauthorized("auth.invalid_refresh_token", "The refresh token is invalid or has been revoked.");

    public static readonly Error InvalidEmailConfirmationToken =
        Error.Validation("auth.invalid_email_token", "The email confirmation link is invalid or has expired.");

    public static Error Identity(string description) =>
        Error.Validation("auth.identity_error", description);
}
