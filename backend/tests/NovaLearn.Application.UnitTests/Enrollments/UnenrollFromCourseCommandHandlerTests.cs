using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Enrollments.UnenrollFromCourse;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Enrollments;

public sealed class UnenrollFromCourseCommandHandlerTests
{
    private static readonly DateTimeOffset EnrolledAt = new(2026, 4, 2, 8, 0, 0, TimeSpan.Zero);

    private readonly IEnrollmentRepository _enrollments = Substitute.For<IEnrollmentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly UnenrollFromCourseCommandHandler _sut;

    public UnenrollFromCourseCommandHandlerTests() =>
        _sut = new UnenrollFromCourseCommandHandler(_enrollments, _unitOfWork, _currentUser);

    private Enrollment GivenEnrollmentOwnedBy(Guid studentId)
    {
        Enrollment enrollment = Enrollment.Create(studentId, Guid.NewGuid(), EnrolledAt);
        _enrollments.GetByIdAsync(enrollment.Id, Arg.Any<CancellationToken>()).Returns(enrollment);
        return enrollment;
    }

    [Fact]
    public async Task A_student_can_drop_their_own_enrollment()
    {
        Guid studentId = Guid.NewGuid();
        _currentUser.UserId.Returns(studentId);
        Enrollment enrollment = GivenEnrollmentOwnedBy(studentId);

        Result result = await _sut.Handle(
            new UnenrollFromCourseCommand(enrollment.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        enrollment.Status.Should().Be(EnrollmentStatus.Dropped);
        _enrollments.Received(1).Remove(enrollment);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_student_cannot_drop_someone_elses_enrollment()
    {
        _currentUser.UserId.Returns(Guid.NewGuid());
        Enrollment enrollment = GivenEnrollmentOwnedBy(Guid.NewGuid());

        Result result = await _sut.Handle(
            new UnenrollFromCourseCommand(enrollment.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EnrollmentErrors.NotOwner);
        enrollment.Status.Should().Be(EnrollmentStatus.Active);
        _enrollments.DidNotReceive().Remove(Arg.Any<Enrollment>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_administrator_can_drop_any_enrollment()
    {
        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.IsInRole(Roles.Administrator).Returns(true);
        Enrollment enrollment = GivenEnrollmentOwnedBy(Guid.NewGuid());

        Result result = await _sut.Handle(
            new UnenrollFromCourseCommand(enrollment.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        enrollment.Status.Should().Be(EnrollmentStatus.Dropped);
        _enrollments.Received(1).Remove(enrollment);
    }

    [Fact]
    public async Task Dropping_a_missing_enrollment_reports_not_found()
    {
        Guid enrollmentId = Guid.NewGuid();
        _currentUser.UserId.Returns(Guid.NewGuid());
        _enrollments.GetByIdAsync(enrollmentId, Arg.Any<CancellationToken>()).Returns((Enrollment?)null);

        Result result = await _sut.Handle(
            new UnenrollFromCourseCommand(enrollmentId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EnrollmentErrors.NotFound);
    }
}
