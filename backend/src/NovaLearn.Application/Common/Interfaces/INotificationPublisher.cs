using NovaLearn.Application.Features.Notifications.Common;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>
/// Pushes a notification to whoever is connected right now. Implemented over SignalR in the
/// presentation layer, since live delivery is a transport concern.
///
/// Delivery is best effort by design: the notification is already persisted before this is
/// called, so a disconnected recipient still sees it on their next page load.
/// </summary>
public interface INotificationPublisher
{
    Task PublishAsync(Guid recipientId, NotificationDto notification, CancellationToken cancellationToken);

    /// <summary>Pushes the recipient's new unread count, so the badge updates without a refetch.</summary>
    Task PublishUnreadCountAsync(Guid recipientId, int unreadCount, CancellationToken cancellationToken);
}
