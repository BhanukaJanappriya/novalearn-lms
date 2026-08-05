using Microsoft.AspNetCore.SignalR;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Notifications.Common;

namespace NovaLearn.API.Features.Notifications;

/// <summary>
/// SignalR implementation of the live-delivery port. Lives in the presentation layer because
/// which transport carries a notification is not an application concern.
/// </summary>
internal sealed class SignalRNotificationPublisher(IHubContext<NotificationHub> hub)
    : INotificationPublisher
{
    /// <summary>Client method names. Must match the handlers the frontend registers.</summary>
    private const string NotificationReceived = "notificationReceived";
    private const string UnreadCountChanged = "unreadCountChanged";

    public Task PublishAsync(
        Guid recipientId, NotificationDto notification, CancellationToken cancellationToken) =>
        hub.Clients
            .Group(NotificationHub.GroupFor(recipientId))
            .SendAsync(NotificationReceived, notification, cancellationToken);

    public Task PublishUnreadCountAsync(
        Guid recipientId, int unreadCount, CancellationToken cancellationToken) =>
        hub.Clients
            .Group(NotificationHub.GroupFor(recipientId))
            .SendAsync(UnreadCountChanged, unreadCount, cancellationToken);
}
