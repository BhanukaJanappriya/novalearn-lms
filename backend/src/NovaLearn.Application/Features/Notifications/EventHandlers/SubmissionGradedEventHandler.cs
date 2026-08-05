using MediatR;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Notifications.Common;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Assessments.Events;
using NovaLearn.Domain.Notifications;

namespace NovaLearn.Application.Features.Notifications.EventHandlers;

/// <summary>Tells the learner their work has been marked.</summary>
public sealed class SubmissionGradedEventHandler(
    IAssessmentRepository assessments,
    NotificationDispatcher dispatcher)
    : INotificationHandler<SubmissionGradedDomainEvent>
{
    public async Task Handle(SubmissionGradedDomainEvent notification, CancellationToken cancellationToken)
    {
        Assignment? assignment =
            await assessments.GetAssignmentAsync(notification.AssignmentId, cancellationToken);

        if (assignment is null)
        {
            return;
        }

        Notification item = Notification.Create(
            notification.StudentId,
            NotificationType.SubmissionGraded,
            "Work marked",
            $"{assignment.Title}: {notification.PointsAwarded} out of {notification.MaxPoints}.",
            $"/my-courses/{assignment.CourseId}/assignments");

        await dispatcher.DispatchAsync([item], cancellationToken);
    }
}
