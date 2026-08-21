using MediatR;
using NovaLearn.Application.Features.Notifications.Common;
using NovaLearn.Domain.Notifications;
using NovaLearn.Domain.Support.Events;

namespace NovaLearn.Application.Features.Notifications.EventHandlers;

/// <summary>
/// Tells the other side of a ticket's conversation that a reply landed.
///
/// Deliberately does not load the ticket again: domain events dispatch from inside the interceptor
/// that runs immediately after the triggering save commits, still on the same DbContext, and
/// re-querying an entity that context is still holding tracked mid-dispatch produced a spurious
/// EF concurrency exception here. Everything this handler needs travels on the event itself.
/// </summary>
public sealed class SupportTicketRepliedEventHandler(NotificationDispatcher dispatcher)
    : INotificationHandler<SupportTicketRepliedDomainEvent>
{
    public async Task Handle(SupportTicketRepliedDomainEvent notification, CancellationToken cancellationToken)
    {
        if (notification.RecipientId is not { } recipientId)
        {
            // Nobody has claimed the ticket yet, so a submitter's reply has no specific staff
            // member to tell — they will see it when they open the queue.
            return;
        }

        // The link differs by audience: the submitter's own ticket page, or the staff queue's.
        string link = recipientId == notification.SubmittedById
            ? $"/support/{notification.TicketId}"
            : $"/admin/support/{notification.TicketId}";

        Notification item = Notification.Create(
            recipientId,
            NotificationType.SupportTicketReplied,
            "New reply",
            $"{notification.Subject}: there's a new reply.",
            link);

        await dispatcher.DispatchAsync([item], cancellationToken);
    }
}
