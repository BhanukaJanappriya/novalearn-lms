using MediatR;
using NovaLearn.Application.Features.Notifications.Common;
using NovaLearn.Domain.Notifications;
using NovaLearn.Domain.Payments.Events;

namespace NovaLearn.Application.Features.Notifications.EventHandlers;

/// <summary>Tells the learner their payment went through and they are enrolled.</summary>
public sealed class PaymentSucceededEventHandler(NotificationDispatcher dispatcher)
    : INotificationHandler<PaymentSucceededDomainEvent>
{
    public async Task Handle(PaymentSucceededDomainEvent notification, CancellationToken cancellationToken)
    {
        Notification item = Notification.Create(
            notification.StudentId,
            NotificationType.PaymentSucceeded,
            "Payment confirmed",
            $"You're enrolled in {notification.CourseTitle}.",
            "/my-courses");

        await dispatcher.DispatchAsync([item], cancellationToken);
    }
}
