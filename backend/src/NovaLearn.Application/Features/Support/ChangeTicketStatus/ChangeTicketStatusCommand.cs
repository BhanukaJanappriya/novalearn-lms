using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Support.Common;
using NovaLearn.Domain.Support;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Support.ChangeTicketStatus;

/// <summary>Moves a ticket to a new status. Staff only.</summary>
public sealed record ChangeTicketStatusCommand(Guid TicketId, TicketStatus Status)
    : IRequest<Result<TicketDetailDto>>;

public sealed class ChangeTicketStatusCommandHandler(
    ISupportTicketRepository tickets,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeTicketStatusCommand, Result<TicketDetailDto>>
{
    public async Task<Result<TicketDetailDto>> Handle(
        ChangeTicketStatusCommand request, CancellationToken cancellationToken)
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

        ticket.ChangeStatus(request.Status, dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(SupportTicketMapper.ToDetailDto(ticket, includeInternalNotes: true));
    }
}
