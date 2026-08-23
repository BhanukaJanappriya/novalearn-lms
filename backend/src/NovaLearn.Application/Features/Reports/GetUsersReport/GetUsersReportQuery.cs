using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Reports.Common;
using NovaLearn.Domain.Reports;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Reports.GetUsersReport;

/// <summary>Every account, for export. Reuses the user directory's own search. Staff only.</summary>
public sealed record GetUsersReportQuery(string? Search, string? Role, bool? IsActive, bool? EmailConfirmed)
    : IRequest<Result<IReadOnlyList<AdminUserRow>>>;

public sealed class GetUsersReportQueryHandler(
    IUserDirectory users,
    IReportRunRepository reportRuns,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : IRequestHandler<GetUsersReportQuery, Result<IReadOnlyList<AdminUserRow>>>
{
    public async Task<Result<IReadOnlyList<AdminUserRow>>> Handle(
        GetUsersReportQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId || !ReportAuthority.IsStaff(currentUser))
        {
            return Result.Failure<IReadOnlyList<AdminUserRow>>(ReportErrors.StaffOnly);
        }

        PagedResult<AdminUserRow> page = await users.SearchAsync(
            request.Search,
            request.Role,
            request.IsActive,
            request.EmailConfirmed,
            page: 1,
            ReportExport.MaxRows,
            cancellationToken);

        string? filters = ReportFilters.Summarize(
            ("search", request.Search),
            ("role", request.Role),
            ("active", request.IsActive),
            ("verified", request.EmailConfirmed));

        await reportRuns.AddAsync(
            ReportRun.Create(ReportType.Users, callerId, filters, page.Items.Count, dateTimeProvider.UtcNow),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(page.Items);
    }
}
