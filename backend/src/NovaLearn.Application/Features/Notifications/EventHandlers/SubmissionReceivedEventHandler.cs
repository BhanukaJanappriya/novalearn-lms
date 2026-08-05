using MediatR;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Notifications.Common;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Assessments.Events;
using NovaLearn.Domain.Notifications;

namespace NovaLearn.Application.Features.Notifications.EventHandlers;

/// <summary>
/// Tells the course owner there is something to mark. The learner is not told, since they just
/// pressed the button.
/// </summary>
public sealed class SubmissionReceivedEventHandler(
    IAssessmentRepository assessments,
    IUserDirectory users,
    NotificationDispatcher dispatcher)
    : INotificationHandler<SubmissionReceivedDomainEvent>
{
    public async Task Handle(SubmissionReceivedDomainEvent notification, CancellationToken cancellationToken)
    {
        Assignment? assignment =
            await assessments.GetAssignmentAsync(notification.AssignmentId, cancellationToken);

        if (assignment?.Course is null)
        {
            return;
        }

        // The roster page shows who submitted, so the name is worth carrying in the message.
        var student = await users.GetAsync(notification.StudentId, cancellationToken);
        string who = student?.FullName ?? "A learner";
        string late = notification.IsLate ? " (late)" : string.Empty;

        Notification item = Notification.Create(
            assignment.Course.LecturerId,
            NotificationType.SubmissionReceived,
            "Work handed in",
            $"{who} submitted {assignment.Title}{late}.",
            $"/admin/courses/{assignment.CourseId}/assignments");

        await dispatcher.DispatchAsync([item], cancellationToken);
    }
}
