using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Support.Common;
using NovaLearn.Domain.Support;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Support.GetMyTickets;

/// <summary>The caller's own tickets, newest activity first.</summary>
public sealed record GetMyTicketsQuery : IRequest<Result<IReadOnlyList<TicketSummaryDto>>>;

public sealed class GetMyTicketsQueryHandler(ISupportTicketRepository tickets, ICurrentUser currentUser)
    : IRequestHandler<GetMyTicketsQuery, Result<IReadOnlyList<TicketSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<TicketSummaryDto>>> Handle(
        GetMyTicketsQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId)
        {
            return Result.Failure<IReadOnlyList<TicketSummaryDto>>(SupportErrors.Unauthenticated);
        }

        IReadOnlyList<SupportTicket> mine = await tickets.ListForUserAsync(callerId, cancellationToken);

        return Result.Success<IReadOnlyList<TicketSummaryDto>>(
            mine.Select(SupportTicketMapper.ToSummaryDto).ToList());
    }
}
