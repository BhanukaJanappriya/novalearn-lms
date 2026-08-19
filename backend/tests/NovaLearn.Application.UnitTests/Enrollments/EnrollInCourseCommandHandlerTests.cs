using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Enrollments.Common;
using NovaLearn.Application.Features.Enrollments.EnrollInCourse;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Enrollments;

public sealed class EnrollInCourseCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly ICourseRepository _courses = Substitute.For<ICourseRepository>();
    private readonly IEnrollmentRepository _enrollments = Substitute.For<IEnrollmentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly Guid _studentId = Guid.NewGuid();
    private readonly EnrollInCourseCommandHandler _sut;

    public EnrollInCourseCommandHandlerTests()
    {
        _currentUser.UserId.Returns(_studentId);
        _clock.UtcNow.Returns(Now);
        _sut = new EnrollInCourseCommandHandler(_courses, _enrollments, _unitOfWork, _currentUser, _clock);
    }

    private static Course SampleCourse(CourseStatus status) => Course.Create(
        "Introduction to Programming", "CS101", "Fundamentals", "Computer Science",
        CourseLevel.Beginner, status, 0m, null, Guid.NewGuid());

    [Fact]
    public async Task Enrolling_in_a_published_course_creates_an_active_enrollment()
    {
        Course course = SampleCourse(CourseStatus.Published);
        _courses.GetByIdAsync(course.Id, Arg.Any<CancellationToken>()).Returns(course);
        _enrollments.GetActiveAsync(_studentId, course.Id, Arg.Any<CancellationToken>())
            .Returns((Enrollment?)null);

        Result<EnrollmentDto> result =
            await _sut.Handle(new EnrollInCourseCommand(course.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CourseId.Should().Be(course.Id);
        result.Value.StudentId.Should().Be(_studentId);
        result.Value.Status.Should().Be(nameof(EnrollmentStatus.Active));
        result.Value.ProgressPercent.Should().Be(0);
        result.Value.EnrolledAtUtc.Should().Be(Now);

        await _enrollments.Received(1).AddAsync(Arg.Any<Enrollment>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Enrolling_directly_in_a_priced_course_is_rejected()
    {
        // Free courses are enrolled here directly; a priced course goes through checkout, which
        // creates the enrolment itself once payment is confirmed.
        Course course = Course.Create(
            "Intro to Programming", "CS101", "Fundamentals", "Computer Science",
            CourseLevel.Beginner, CourseStatus.Published, 49.99m, null, Guid.NewGuid());
        _courses.GetByIdAsync(course.Id, Arg.Any<CancellationToken>()).Returns(course);

        Result<EnrollmentDto> result =
            await _sut.Handle(new EnrollInCourseCommand(course.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EnrollmentErrors.PaymentRequired);

        await _enrollments.DidNotReceive().AddAsync(Arg.Any<Enrollment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Enrolling_in_an_unpublished_course_is_rejected()
    {
        Course course = SampleCourse(CourseStatus.Draft);
        _courses.GetByIdAsync(course.Id, Arg.Any<CancellationToken>()).Returns(course);

        Result<EnrollmentDto> result =
            await _sut.Handle(new EnrollInCourseCommand(course.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EnrollmentErrors.CourseNotPublished);

        await _enrollments.DidNotReceive().AddAsync(Arg.Any<Enrollment>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Enrolling_twice_in_the_same_course_is_rejected()
    {
        Course course = SampleCourse(CourseStatus.Published);
        _courses.GetByIdAsync(course.Id, Arg.Any<CancellationToken>()).Returns(course);
        _enrollments.GetActiveAsync(_studentId, course.Id, Arg.Any<CancellationToken>())
            .Returns(Enrollment.Create(_studentId, course.Id, Now.AddDays(-10)));

        Result<EnrollmentDto> result =
            await _sut.Handle(new EnrollInCourseCommand(course.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EnrollmentErrors.AlreadyEnrolled);

        await _enrollments.DidNotReceive().AddAsync(Arg.Any<Enrollment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Enrolling_in_a_missing_course_reports_not_found()
    {
        Guid courseId = Guid.NewGuid();
        _courses.GetByIdAsync(courseId, Arg.Any<CancellationToken>()).Returns((Course?)null);

        Result<EnrollmentDto> result =
            await _sut.Handle(new EnrollInCourseCommand(courseId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CourseErrors.NotFound);
    }

    [Fact]
    public async Task An_unauthenticated_caller_cannot_enroll()
    {
        _currentUser.UserId.Returns((Guid?)null);

        Result<EnrollmentDto> result =
            await _sut.Handle(new EnrollInCourseCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EnrollmentErrors.Unauthenticated);
    }
}
