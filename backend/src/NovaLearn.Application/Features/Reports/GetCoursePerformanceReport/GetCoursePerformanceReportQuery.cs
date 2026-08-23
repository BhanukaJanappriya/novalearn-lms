using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Reports.Common;
using NovaLearn.Domain.Reports;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Reports.GetCoursePerformanceReport;

/// <summary>
/// How every course has done, across its whole life. Reuses platform analytics' own course
/// breakdown, which is already computed over each course's lifetime regardless of the window
/// requested — see the remarks on <c>PlatformAnalyticsService.BuildCoursesAsync</c> — so no new
/// query is needed here at all, only a request for its course rows. Staff only.
/// </summary>
public sealed record GetCoursePerformanceReportQuery : IRequest<Result<IReadOnlyList<CoursePerformanceRow>>>;

public sealed class GetCoursePerformanceReportQueryHandler(
    IPlatformAnalytics platformAnalytics,
    IReportRunRepository reportRuns,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : IRequestHandler<GetCoursePerformanceReportQuery, Result<IReadOnlyList<CoursePerformanceRow>>>
{
    public async Task<Result<IReadOnlyList<CoursePerformanceRow>>> Handle(
        GetCoursePerformanceReportQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId || !ReportAuthority.IsStaff(currentUser))
        {
            return Result.Failure<IReadOnlyList<CoursePerformanceRow>>(ReportErrors.StaffOnly);
        }

        PlatformAnalytics analytics = await platformAnalytics.GetAsync(days: 30, cancellationToken);

        await reportRuns.AddAsync(
            ReportRun.Create(
                ReportType.CoursePerformance, callerId, filtersSummary: null, analytics.Courses.Count,
                dateTimeProvider.UtcNow),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(analytics.Courses);
    }
}
