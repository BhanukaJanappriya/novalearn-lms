using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Support.Common;
using NovaLearn.Domain.Support;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Support.GetStaffTickets;

/// <summary>The staff queue: every ticket, paged and optionally filtered. Staff only.</summary>
public sealed record GetStaffTicketsQuery(
    TicketStatus? Status,
    TicketCategory? Category,
    TicketPriority? Priority,
    Guid? AssignedToId,
    string? Search,
    int Page,
    int PageSize) : IRequest<Result<PagedResult<TicketSummaryDto>>>;

public sealed class GetStaffTicketsQueryHandler(ISupportTicketRepository tickets, ICurrentUser currentUser)
    : IRequestHandler<GetStaffTicketsQuery, Result<PagedResult<TicketSummaryDto>>>
{
    public async Task<Result<PagedResult<TicketSummaryDto>>> Handle(
        GetStaffTicketsQuery request, CancellationToken cancellationToken)
    {
        if (!SupportAuthority.IsStaff(currentUser))
        {
            return Result.Failure<PagedResult<TicketSummaryDto>>(SupportErrors.StaffOnly);
        }

        PagedResult<SupportTicket> page = await tickets.ListForStaffAsync(
            request.Status,
            request.Category,
            request.Priority,
            request.AssignedToId,
            request.Search,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result.Success(new PagedResult<TicketSummaryDto>(
            page.Items.Select(SupportTicketMapper.ToSummaryDto).ToList(),
            page.Page,
            page.PageSize,
            page.TotalCount));
    }
}
