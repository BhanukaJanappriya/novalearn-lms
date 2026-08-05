using Microsoft.Extensions.Logging;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Notifications;

namespace NovaLearn.Application.Features.Notifications.Common;

/// <summary>
/// Persists notifications and then pushes them to anyone connected. Shared by every domain event
/// handler so the store-then-push order is written once.
///
/// The push is best effort: it happens after the rows are saved, and a transport failure is
/// logged rather than thrown, because losing a live toast must never undo a grade.
/// </summary>
public sealed class NotificationDispatcher(
    INotificationRepository notifications,
    INotificationPublisher publisher,
    IUnitOfWork unitOfWork,
    ILogger<NotificationDispatcher> logger)
{
    public async Task DispatchAsync(
        IReadOnlyList<Notification> batch, CancellationToken cancellationToken)
    {
        if (batch.Count == 0)
        {
            return;
        }

        await notifications.AddRangeAsync(batch, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (Notification notification in batch)
        {
            await PushAsync(notification, cancellationToken);
        }
    }

    private async Task PushAsync(Notification notification, CancellationToken cancellationToken)
    {
        try
        {
            await publisher.PublishAsync(
                notification.RecipientId, NotificationDto.FromEntity(notification), cancellationToken);

            int unread = await notifications.CountUnreadAsync(notification.RecipientId, cancellationToken);
            await publisher.PublishUnreadCountAsync(notification.RecipientId, unread, cancellationToken);
        }
        catch (Exception ex)
        {
            // The notification is already stored, so the recipient still sees it on next load.
            logger.LogWarning(
                ex,
                "Live delivery failed for notification {NotificationId} to {RecipientId}",
                notification.Id,
                notification.RecipientId);
        }
    }
}
