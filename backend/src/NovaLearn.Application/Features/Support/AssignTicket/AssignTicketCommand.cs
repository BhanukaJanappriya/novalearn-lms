using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Support.Common;
using NovaLearn.Domain.Support;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Support.AssignTicket;

/// <summary>Claims a ticket for a staff member, hands it to someone else, or unassigns it (null). Staff only.</summary>
public sealed record AssignTicketCommand(Guid TicketId, Guid? AssignedToId) : IRequest<Result<TicketDetailDto>>;

public sealed class AssignTicketCommandHandler(
    ISupportTicketRepository tickets, ICurrentUser currentUser, IUnitOfWork unitOfWork)
    : IRequestHandler<AssignTicketCommand, Result<TicketDetailDto>>
{
    public async Task<Result<TicketDetailDto>> Handle(
        AssignTicketCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure<TicketDetailDto>(SupportErrors.Unauthenticated);
        }

        if (!SupportAuthority.IsStaff(currentUser))
        {
            return Result.Failure<TicketDetailDto>(SupportErrors.StaffOnly);
        }

        SupportTicket? ticket = await tickets.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Failure<TicketDetailDto>(SupportErrors.NotFound);
        }

        if (request.AssignedToId is { } assignee)
        {
            ticket.AssignTo(assignee);
        }
        else
        {
            ticket.Unassign();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        SupportTicket saved = await tickets.GetByIdAsync(ticket.Id, cancellationToken) ?? ticket;

        return Result.Success(SupportTicketMapper.ToDetailDto(saved, includeInternalNotes: true));
    }
}
