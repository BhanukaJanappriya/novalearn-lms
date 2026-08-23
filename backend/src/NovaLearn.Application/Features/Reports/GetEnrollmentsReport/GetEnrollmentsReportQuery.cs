using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Reports.Common;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Reports;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Reports.GetEnrollmentsReport;

/// <summary>Every enrolment, flattened for export. Staff only.</summary>
public sealed record GetEnrollmentsReportQuery(
    EnrollmentStatus? Status, DateTimeOffset? FromUtc, DateTimeOffset? ToUtc)
    : IRequest<Result<IReadOnlyList<EnrollmentReportRow>>>;

public sealed class GetEnrollmentsReportQueryHandler(
    IReportsRepository reports,
    IReportRunRepository reportRuns,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : IRequestHandler<GetEnrollmentsReportQuery, Result<IReadOnlyList<EnrollmentReportRow>>>
{
    public async Task<Result<IReadOnlyList<EnrollmentReportRow>>> Handle(
        GetEnrollmentsReportQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId || !ReportAuthority.IsStaff(currentUser))
        {
            return Result.Failure<IReadOnlyList<EnrollmentReportRow>>(ReportErrors.StaffOnly);
        }

        IReadOnlyList<EnrollmentReportRow> rows = await reports.ListEnrollmentsAsync(
            request.Status, request.FromUtc, request.ToUtc, cancellationToken);

        string? filters = ReportFilters.Summarize(
            ("status", request.Status), ("from", request.FromUtc), ("to", request.ToUtc));

        await reportRuns.AddAsync(
            ReportRun.Create(ReportType.Enrollments, callerId, filters, rows.Count, dateTimeProvider.UtcNow),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(rows);
    }
}
