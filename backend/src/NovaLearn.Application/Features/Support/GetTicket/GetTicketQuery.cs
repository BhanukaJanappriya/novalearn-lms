using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Support.Common;
using NovaLearn.Domain.Support;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Support.GetTicket;

/// <summary>A single ticket with its thread. The submitter or staff may view it.</summary>
public sealed record GetTicketQuery(Guid TicketId) : IRequest<Result<TicketDetailDto>>;

public sealed class GetTicketQueryHandler(ISupportTicketRepository tickets, ICurrentUser currentUser)
    : IRequestHandler<GetTicketQuery, Result<TicketDetailDto>>
{
    public async Task<Result<TicketDetailDto>> Handle(
        GetTicketQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId)
        {
            return Result.Failure<TicketDetailDto>(SupportErrors.Unauthenticated);
        }

        SupportTicket? ticket = await tickets.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Failure<TicketDetailDto>(SupportErrors.NotFound);
        }

        bool isStaff = SupportAuthority.IsStaff(currentUser);

        if (!isStaff && ticket.SubmittedById != callerId)
        {
            return Result.Failure<TicketDetailDto>(SupportErrors.NotOwnerOrStaff);
        }

        return Result.Success(SupportTicketMapper.ToDetailDto(ticket, includeInternalNotes: isStaff));
    }
}
