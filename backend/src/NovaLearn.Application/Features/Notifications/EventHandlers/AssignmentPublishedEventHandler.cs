using MediatR;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Notifications.Common;
using NovaLearn.Domain.Assessments.Events;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Notifications;

namespace NovaLearn.Application.Features.Notifications.EventHandlers;

/// <summary>Tells everyone on the course that there is new work to do.</summary>
public sealed class AssignmentPublishedEventHandler(
    IEnrollmentRepository enrollments,
    NotificationDispatcher dispatcher)
    : INotificationHandler<AssignmentPublishedDomainEvent>
{
    public async Task Handle(AssignmentPublishedDomainEvent notification, CancellationToken cancellationToken)
    {
        IReadOnlyList<Enrollment> roster =
            await enrollments.ListForCourseAsync(notification.CourseId, cancellationToken);

        string due = notification.DueAtUtc is { } dueAt
            ? $" Due {dueAt.ToLocalTime():d MMM}."
            : string.Empty;

        List<Notification> batch = roster
            // Someone who dropped the course should not be told about its new work.
            .Where(e => e.Status != EnrollmentStatus.Dropped)
            .Select(e => Notification.Create(
                e.StudentId,
                NotificationType.AssignmentPublished,
                "New assignment",
                $"{notification.Title} has been set.{due}",
                $"/my-courses/{notification.CourseId}/assignments"))
            .ToList();

        await dispatcher.DispatchAsync(batch, cancellationToken);
    }
}
