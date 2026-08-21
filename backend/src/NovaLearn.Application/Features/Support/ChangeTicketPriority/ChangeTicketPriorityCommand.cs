using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Support.Common;
using NovaLearn.Domain.Support;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Support.ChangeTicketPriority;

/// <summary>Re-prioritises a ticket. Staff only.</summary>
public sealed record ChangeTicketPriorityCommand(Guid TicketId, TicketPriority Priority)
    : IRequest<Result<TicketDetailDto>>;

public sealed class ChangeTicketPriorityCommandHandler(
    ISupportTicketRepository tickets, ICurrentUser currentUser, IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeTicketPriorityCommand, Result<TicketDetailDto>>
{
    public async Task<Result<TicketDetailDto>> Handle(
        ChangeTicketPriorityCommand request, CancellationToken cancellationToken)
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

        ticket.ChangePriority(request.Priority);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(SupportTicketMapper.ToDetailDto(ticket, includeInternalNotes: true));
    }
}
