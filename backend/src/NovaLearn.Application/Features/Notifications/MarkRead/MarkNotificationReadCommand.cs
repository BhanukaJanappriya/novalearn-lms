using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Notifications;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Notifications.MarkRead;

/// <summary>Marks one notification read.</summary>
public sealed record MarkNotificationReadCommand(Guid NotificationId) : IRequest<Result>;

public sealed class MarkNotificationReadCommandHandler(
    INotificationRepository notifications,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider dateTime)
    : IRequestHandler<MarkNotificationReadCommand, Result>
{
    public async Task<Result> Handle(
        MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId)
        {
            return Result.Failure(NotificationErrors.Unauthenticated);
        }

        Notification? notification = await notifications.GetAsync(request.NotificationId, cancellationToken);
        if (notification is null)
        {
            return Result.Failure(NotificationErrors.NotFound);
        }

        // Nobody else's feed, not even an administrator's.
        if (notification.RecipientId != callerId)
        {
            return Result.Failure(NotificationErrors.NotRecipient);
        }

        notification.MarkRead(dateTime.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
