using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Notifications;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Notifications.MarkRead;

/// <summary>Clears the caller's whole unread queue.</summary>
public sealed record MarkAllNotificationsReadCommand : IRequest<Result>;

public sealed class MarkAllNotificationsReadCommandHandler(
    INotificationRepository notifications,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider dateTime)
    : IRequestHandler<MarkAllNotificationsReadCommand, Result>
{
    public async Task<Result> Handle(
        MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId)
        {
            return Result.Failure(NotificationErrors.Unauthenticated);
        }

        IReadOnlyList<Notification> unread =
            await notifications.ListUnreadAsync(callerId, cancellationToken);

        if (unread.Count == 0)
        {
            return Result.Success();
        }

        DateTimeOffset now = dateTime.UtcNow;
        foreach (Notification notification in unread)
        {
            notification.MarkRead(now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
