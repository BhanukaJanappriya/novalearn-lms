using FluentValidation;
using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Support.Common;
using NovaLearn.Domain.Support;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Support.ReplyToTicket;

/// <summary>
/// Adds a message to a ticket's thread. The submitter or a staff member may reply; only staff may
/// mark a reply as an internal note.
/// </summary>
public sealed record ReplyToTicketCommand(Guid TicketId, string Body, bool IsInternalNote)
    : IRequest<Result<TicketDetailDto>>;

public sealed class ReplyToTicketCommandValidator : AbstractValidator<ReplyToTicketCommand>
{
    public ReplyToTicketCommandValidator()
    {
        RuleFor(c => c.Body).NotEmpty().MaximumLength(SupportTicketMessage.BodyMaxLength);
    }
}

public sealed class ReplyToTicketCommandHandler(
    ISupportTicketRepository tickets,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ReplyToTicketCommand, Result<TicketDetailDto>>
{
    public async Task<Result<TicketDetailDto>> Handle(
        ReplyToTicketCommand request, CancellationToken cancellationToken)
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

        if (request.IsInternalNote && !isStaff)
        {
            return Result.Failure<TicketDetailDto>(SupportErrors.InternalNoteNotAllowed);
        }

        SupportTicketMessage message = ticket.Reply(
            callerId, request.Body, request.IsInternalNote, dateTimeProvider.UtcNow);

        // The ticket was already loaded (tracked), not freshly created, so the new message it now
        // holds needs to be tracked as an insert explicitly — see the remarks on Reply for why.
        await tickets.AddMessageAsync(message, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        SupportTicket saved = await tickets.GetByIdAsync(ticket.Id, cancellationToken) ?? ticket;

        return Result.Success(SupportTicketMapper.ToDetailDto(saved, includeInternalNotes: isStaff));
    }
}
