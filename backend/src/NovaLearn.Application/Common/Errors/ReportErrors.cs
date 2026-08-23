using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Common.Errors;

/// <summary>Central catalogue of reporting failures.</summary>
public static class ReportErrors
{
    public static readonly Error StaffOnly =
        Error.Forbidden("reports.staff_only", "Only an administrator can run reports.");
}
