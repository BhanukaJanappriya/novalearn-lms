using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Reports;

namespace NovaLearn.Application.Features.Reports.Common;

/// <summary>One enrolment, flattened for export — the only report row with no existing read model to reuse.</summary>
public sealed record EnrollmentReportRow(
    Guid EnrollmentId,
    Guid StudentId,
    string StudentName,
    string StudentEmail,
    Guid CourseId,
    string CourseTitle,
    EnrollmentStatus Status,
    int ProgressPercent,
    DateTimeOffset EnrolledAtUtc,
    DateTimeOffset? CompletedAtUtc);

/// <summary>One entry in the "recent report runs" audit panel.</summary>
public sealed record ReportRunDto(
    Guid Id,
    ReportType Type,
    string GeneratedByName,
    string? FiltersSummary,
    int RowCount,
    DateTimeOffset CreatedAtUtc);

public static class ReportRunMapper
{
    public static ReportRunDto ToDto(ReportRun run) =>
        new(
            run.Id,
            run.Type,
            run.GeneratedBy is { } user ? $"{user.FirstName} {user.LastName}".Trim() : "Unknown",
            run.FiltersSummary,
            run.RowCount,
            run.CreatedAtUtc);
}
