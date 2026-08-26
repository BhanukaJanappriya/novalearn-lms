using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Common.Errors;

/// <summary>Central catalogue of audit log failures.</summary>
public static class AuditErrors
{
    public static readonly Error StaffOnly =
        Error.Forbidden("audit.staff_only", "Only an administrator can view the audit log.");
}
