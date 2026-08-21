using MediatR;
using NovaLearn.Application.Features.Notifications.Common;
using NovaLearn.Domain.Notifications;
using NovaLearn.Domain.Support;
using NovaLearn.Domain.Support.Events;

namespace NovaLearn.Application.Features.Notifications.EventHandlers;

/// <summary>Tells the submitter their ticket's status changed.</summary>
public sealed class SupportTicketStatusChangedEventHandler(NotificationDispatcher dispatcher)
    : INotificationHandler<SupportTicketStatusChangedDomainEvent>
{
    public async Task Handle(
        SupportTicketStatusChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        Notification item = Notification.Create(
            notification.SubmittedById,
            NotificationType.SupportTicketStatusChanged,
            "Ticket updated",
            $"{notification.Subject} is now {Describe(notification.Status)}.",
            $"/support/{notification.TicketId}");

        await dispatcher.DispatchAsync([item], cancellationToken);
    }

    private static string Describe(TicketStatus status) =>
        status switch
        {
            TicketStatus.InProgress => "in progress",
            _ => status.ToString().ToLowerInvariant()
        };
}
