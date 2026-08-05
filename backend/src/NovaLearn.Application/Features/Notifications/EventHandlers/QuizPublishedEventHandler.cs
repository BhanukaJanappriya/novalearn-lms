using MediatR;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Notifications.Common;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Notifications;
using NovaLearn.Domain.Quizzes.Events;

namespace NovaLearn.Application.Features.Notifications.EventHandlers;

/// <summary>Tells everyone on the course that a new quiz is open.</summary>
public sealed class QuizPublishedEventHandler(
    IEnrollmentRepository enrollments,
    NotificationDispatcher dispatcher)
    : INotificationHandler<QuizPublishedDomainEvent>
{
    public async Task Handle(QuizPublishedDomainEvent notification, CancellationToken cancellationToken)
    {
        IReadOnlyList<Enrollment> roster =
            await enrollments.ListForCourseAsync(notification.CourseId, cancellationToken);

        string timing = notification.TimeLimitMinutes is { } minutes
            ? $" You get {minutes} minutes."
            : string.Empty;

        List<Notification> batch = roster
            .Where(e => e.Status != EnrollmentStatus.Dropped)
            .Select(e => Notification.Create(
                e.StudentId,
                NotificationType.QuizPublished,
                "New quiz",
                $"{notification.Title} is open, {notification.QuestionCount} question" +
                    $"{(notification.QuestionCount == 1 ? string.Empty : "s")}.{timing}",
                $"/my-courses/{notification.CourseId}/quizzes"))
            .ToList();

        await dispatcher.DispatchAsync(batch, cancellationToken);
    }
}
