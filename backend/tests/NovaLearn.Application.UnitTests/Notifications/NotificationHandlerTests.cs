using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Notifications.Common;
using NovaLearn.Application.Features.Notifications.EventHandlers;
using NovaLearn.Application.Features.Notifications.MarkRead;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Assessments.Events;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Notifications;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Notifications;

public sealed class NotificationHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 5, 12, 0, 0, TimeSpan.Zero);

    private readonly INotificationRepository _notifications = Substitute.For<INotificationRepository>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IEnrollmentRepository _enrollments = Substitute.For<IEnrollmentRepository>();
    private readonly IAssessmentRepository _assessments = Substitute.For<IAssessmentRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
    private readonly NotificationDispatcher _dispatcher;

    public NotificationHandlerTests()
    {
        _dateTime.UtcNow.Returns(Now);
        _dispatcher = new NotificationDispatcher(
            _notifications, _publisher, _unitOfWork, NullLogger<NotificationDispatcher>.Instance);
    }

    private static Enrollment EnrolledStudent(Guid courseId, EnrollmentStatus status = EnrollmentStatus.Active)
    {
        Enrollment enrollment = Enrollment.Create(Guid.NewGuid(), courseId, Now.AddDays(-10));
        if (status == EnrollmentStatus.Dropped)
        {
            enrollment.Drop();
        }

        return enrollment;
    }

    // --- Fan-out ------------------------------------------------------------------------

    [Fact]
    public async Task Publishing_an_assignment_notifies_every_active_learner()
    {
        Guid courseId = Guid.NewGuid();
        _enrollments.ListForCourseAsync(courseId, Arg.Any<CancellationToken>())
            .Returns([EnrolledStudent(courseId), EnrolledStudent(courseId), EnrolledStudent(courseId)]);

        var sut = new AssignmentPublishedEventHandler(_enrollments, _dispatcher);

        await sut.Handle(
            new AssignmentPublishedDomainEvent(Guid.NewGuid(), courseId, "Task", null),
            CancellationToken.None);

        await _notifications.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<Notification>>(batch => batch.Count() == 3), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>Someone who left the course should not keep getting its announcements.</summary>
    [Fact]
    public async Task A_learner_who_dropped_the_course_is_not_notified()
    {
        Guid courseId = Guid.NewGuid();
        _enrollments.ListForCourseAsync(courseId, Arg.Any<CancellationToken>())
            .Returns([
                EnrolledStudent(courseId),
                EnrolledStudent(courseId, EnrollmentStatus.Dropped)
            ]);

        var sut = new AssignmentPublishedEventHandler(_enrollments, _dispatcher);

        await sut.Handle(
            new AssignmentPublishedDomainEvent(Guid.NewGuid(), courseId, "Task", null),
            CancellationToken.None);

        await _notifications.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<Notification>>(batch => batch.Count() == 1), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_empty_course_writes_nothing_at_all()
    {
        Guid courseId = Guid.NewGuid();
        _enrollments.ListForCourseAsync(courseId, Arg.Any<CancellationToken>())
            .Returns([]);

        var sut = new AssignmentPublishedEventHandler(_enrollments, _dispatcher);

        await sut.Handle(
            new AssignmentPublishedDomainEvent(Guid.NewGuid(), courseId, "Task", null),
            CancellationToken.None);

        await _notifications.DidNotReceive().AddRangeAsync(
            Arg.Any<IEnumerable<Notification>>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Marking_work_notifies_the_learner_with_their_score()
    {
        Guid courseId = Guid.NewGuid();
        Guid studentId = Guid.NewGuid();
        Assignment assignment = Assignment.Create(
            courseId, "Problem set", null, null, 20, false, AssessmentStatus.Published);

        _assessments.GetAssignmentAsync(assignment.Id, Arg.Any<CancellationToken>()).Returns(assignment);

        var sut = new SubmissionGradedEventHandler(_assessments, _dispatcher);

        await sut.Handle(
            new SubmissionGradedDomainEvent(Guid.NewGuid(), assignment.Id, studentId, 16, 20),
            CancellationToken.None);

        await _notifications.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<Notification>>(batch =>
                batch.Single().RecipientId == studentId
                && batch.Single().Type == NotificationType.SubmissionGraded
                && batch.Single().Message.Contains("16 out of 20")),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// A notification is stored before it is pushed, so losing the live channel must never
    /// undo the grade that triggered it.
    /// </summary>
    [Fact]
    public async Task A_failed_live_push_does_not_bubble_up()
    {
        Guid courseId = Guid.NewGuid();
        _enrollments.ListForCourseAsync(courseId, Arg.Any<CancellationToken>())
            .Returns([EnrolledStudent(courseId)]);

        _publisher
            .PublishAsync(Arg.Any<Guid>(), Arg.Any<NotificationDto>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("hub is down"));

        var sut = new AssignmentPublishedEventHandler(_enrollments, _dispatcher);

        Func<Task> act = () => sut.Handle(
            new AssignmentPublishedDomainEvent(Guid.NewGuid(), courseId, "Task", null),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // --- Reading ------------------------------------------------------------------------

    [Fact]
    public async Task A_learner_can_mark_their_own_notification_read()
    {
        Guid recipientId = Guid.NewGuid();
        Notification notification = Notification.Create(
            recipientId, NotificationType.SubmissionGraded, "Marked", "8 of 10", null);

        _currentUser.UserId.Returns(recipientId);
        _notifications.GetAsync(notification.Id, Arg.Any<CancellationToken>()).Returns(notification);

        var sut = new MarkNotificationReadCommandHandler(
            _notifications, _unitOfWork, _currentUser, _dateTime);

        Result result = await sut.Handle(
            new MarkNotificationReadCommand(notification.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
        notification.ReadAtUtc.Should().Be(Now);
    }

    [Fact]
    public async Task Nobody_can_mark_someone_elses_notification_read()
    {
        Notification notification = Notification.Create(
            Guid.NewGuid(), NotificationType.SubmissionGraded, "Marked", "8 of 10", null);

        _currentUser.UserId.Returns(Guid.NewGuid());
        _notifications.GetAsync(notification.Id, Arg.Any<CancellationToken>()).Returns(notification);

        var sut = new MarkNotificationReadCommandHandler(
            _notifications, _unitOfWork, _currentUser, _dateTime);

        Result result = await sut.Handle(
            new MarkNotificationReadCommand(notification.Id), CancellationToken.None);

        result.Error.Should().Be(NotificationErrors.NotRecipient);
        notification.IsRead.Should().BeFalse();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Marking_read_twice_keeps_the_first_read_time()
    {
        Notification notification = Notification.Create(
            Guid.NewGuid(), NotificationType.QuizPublished, "New quiz", "Open now", null);

        notification.MarkRead(Now);
        notification.MarkRead(Now.AddHours(3));

        notification.ReadAtUtc.Should().Be(Now);
    }
}
