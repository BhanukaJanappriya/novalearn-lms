using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Reports.Common;
using NovaLearn.Domain.Reports;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Reports.GetRecentReportRuns;

/// <summary>The "recent report runs" audit panel: who ran what, when. Staff only.</summary>
public sealed record GetRecentReportRunsQuery(int Count) : IRequest<Result<IReadOnlyList<ReportRunDto>>>;

public sealed class GetRecentReportRunsQueryHandler(
    IReportRunRepository reportRuns, ICurrentUser currentUser)
    : IRequestHandler<GetRecentReportRunsQuery, Result<IReadOnlyList<ReportRunDto>>>
{
    public async Task<Result<IReadOnlyList<ReportRunDto>>> Handle(
        GetRecentReportRunsQuery request, CancellationToken cancellationToken)
    {
        if (!ReportAuthority.IsStaff(currentUser))
        {
            return Result.Failure<IReadOnlyList<ReportRunDto>>(ReportErrors.StaffOnly);
        }

        IReadOnlyList<ReportRun> runs = await reportRuns.ListRecentAsync(request.Count, cancellationToken);

        return Result.Success<IReadOnlyList<ReportRunDto>>(runs.Select(ReportRunMapper.ToDto).ToList());
    }
}
