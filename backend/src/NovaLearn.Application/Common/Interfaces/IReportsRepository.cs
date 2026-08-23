using NovaLearn.Application.Features.Reports.Common;
using NovaLearn.Domain.Enrollments;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>
/// Read-side port for the one report that has no existing read model to reuse. The other four
/// report types (Revenue, CoursePerformance, Users, SupportTickets) are served by calling the
/// repositories/services those features already have, each with a large page size in place of a
/// true unlimited query — an honest reuse of what already exists rather than new plumbing for a
/// need those ports already meet.
/// </summary>
public interface IReportsRepository
{
    Task<IReadOnlyList<EnrollmentReportRow>> ListEnrollmentsAsync(
        EnrollmentStatus? status,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        CancellationToken cancellationToken);
}
