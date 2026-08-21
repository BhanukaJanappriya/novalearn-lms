using FluentValidation;
using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Support.Common;
using NovaLearn.Domain.Support;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Support.CreateTicket;

/// <summary>Raises a new support ticket. Open to any signed-in account.</summary>
public sealed record CreateTicketCommand(
    string Subject, TicketCategory Category, TicketPriority Priority, string Message)
    : IRequest<Result<TicketDetailDto>>;

public sealed class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(c => c.Subject).NotEmpty().MaximumLength(SupportTicket.SubjectMaxLength);
        RuleFor(c => c.Message).NotEmpty().MaximumLength(SupportTicketMessage.BodyMaxLength);
        RuleFor(c => c.Category).IsInEnum();
        RuleFor(c => c.Priority).IsInEnum();
    }
}

public sealed class CreateTicketCommandHandler(
    ISupportTicketRepository tickets,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateTicketCommand, Result<TicketDetailDto>>
{
    public async Task<Result<TicketDetailDto>> Handle(
        CreateTicketCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId)
        {
            return Result.Failure<TicketDetailDto>(SupportErrors.Unauthenticated);
        }

        SupportTicket ticket = SupportTicket.Create(
            callerId, request.Subject, request.Category, request.Priority, request.Message,
            dateTimeProvider.UtcNow);

        await tickets.AddAsync(ticket, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Re-read so the response carries the submitter's name rather than nothing: setting a
        // foreign key does not populate the navigation.
        SupportTicket saved = await tickets.GetByIdAsync(ticket.Id, cancellationToken) ?? ticket;

        // The caller here is always the submitter (there is no such thing as staff creating a
        // ticket on someone else's behalf), so this follows the same rule as every other read:
        // a submitter never sees internal notes, even though none can exist on a ticket this new.
        return Result.Success(SupportTicketMapper.ToDetailDto(saved, includeInternalNotes: false));
    }
}
