using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Common.Errors;

/// <summary>Central catalogue of security-center failures.</summary>
public static class SecurityErrors
{
    public static readonly Error StaffOnly =
        Error.Forbidden("security.staff_only", "Only an administrator can view or manage security.");

    public static readonly Error SessionNotFound =
        Error.NotFound("security.session_not_found", "The requested session was not found.");

    public static readonly Error SessionNotActive =
        Error.Conflict("security.session_not_active", "That session has already ended.");
}
