using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Reports.Common;
using NovaLearn.Application.Features.Support.Common;
using NovaLearn.Domain.Reports;
using NovaLearn.Domain.Support;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Reports.GetSupportTicketsReport;

/// <summary>Every ticket, for export. Reuses the staff queue's own paged query. Staff only.</summary>
public sealed record GetSupportTicketsReportQuery(TicketStatus? Status, TicketCategory? Category, TicketPriority? Priority)
    : IRequest<Result<IReadOnlyList<TicketSummaryDto>>>;

public sealed class GetSupportTicketsReportQueryHandler(
    ISupportTicketRepository tickets,
    IReportRunRepository reportRuns,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : IRequestHandler<GetSupportTicketsReportQuery, Result<IReadOnlyList<TicketSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<TicketSummaryDto>>> Handle(
        GetSupportTicketsReportQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId || !ReportAuthority.IsStaff(currentUser))
        {
            return Result.Failure<IReadOnlyList<TicketSummaryDto>>(ReportErrors.StaffOnly);
        }

        PagedResult<SupportTicket> page = await tickets.ListForStaffAsync(
            request.Status,
            request.Category,
            request.Priority,
            assignedToId: null,
            search: null,
            page: 1,
            ReportExport.MaxRows,
            cancellationToken);

        List<TicketSummaryDto> rows = page.Items.Select(SupportTicketMapper.ToSummaryDto).ToList();

        string? filters = ReportFilters.Summarize(
            ("status", request.Status), ("category", request.Category), ("priority", request.Priority));

        await reportRuns.AddAsync(
            ReportRun.Create(ReportType.SupportTickets, callerId, filters, rows.Count, dateTimeProvider.UtcNow),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success<IReadOnlyList<TicketSummaryDto>>(rows);
    }
}
