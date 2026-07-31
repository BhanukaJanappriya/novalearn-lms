using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Enrollments.Common;
using NovaLearn.Application.Features.Enrollments.UpdateProgress;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Enrollments;

public sealed class UpdateProgressCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 31, 12, 0, 0, TimeSpan.Zero);

    private readonly IEnrollmentRepository _enrollments = Substitute.For<IEnrollmentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
    private readonly Guid _studentId = Guid.NewGuid();
    private readonly UpdateProgressCommandHandler _sut;

    public UpdateProgressCommandHandlerTests()
    {
        _dateTime.UtcNow.Returns(Now);
        _sut = new UpdateProgressCommandHandler(_enrollments, _unitOfWork, _currentUser, _dateTime);
    }

    private Enrollment Arrange(Guid? ownerId = null)
    {
        Enrollment enrollment = Enrollment.Create(ownerId ?? _studentId, Guid.NewGuid(), Now.AddDays(-30));
        _enrollments.GetByIdAsync(enrollment.Id, Arg.Any<CancellationToken>()).Returns(enrollment);
        return enrollment;
    }

    [Fact]
    public async Task A_learner_records_progress_on_their_own_enrollment()
    {
        Enrollment enrollment = Arrange();
        _currentUser.UserId.Returns(_studentId);

        Result<EnrollmentDto> result =
            await _sut.Handle(new UpdateProgressCommand(enrollment.Id, 45), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ProgressPercent.Should().Be(45);
        enrollment.Status.Should().Be(EnrollmentStatus.Active);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reaching_one_hundred_percent_completes_the_enrollment_with_a_stamped_time()
    {
        Enrollment enrollment = Arrange();
        _currentUser.UserId.Returns(_studentId);

        Result<EnrollmentDto> result =
            await _sut.Handle(new UpdateProgressCommand(enrollment.Id, 100), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        enrollment.Status.Should().Be(EnrollmentStatus.Completed);
        enrollment.CompletedAtUtc.Should().Be(Now);
    }

    [Fact]
    public async Task A_learner_cannot_record_progress_on_someone_elses_enrollment()
    {
        Enrollment enrollment = Arrange(ownerId: Guid.NewGuid());
        _currentUser.UserId.Returns(_studentId);
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);

        Result<EnrollmentDto> result =
            await _sut.Handle(new UpdateProgressCommand(enrollment.Id, 50), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EnrollmentErrors.NotOwner);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_administrator_may_correct_anyones_progress()
    {
        Enrollment enrollment = Arrange(ownerId: Guid.NewGuid());
        _currentUser.UserId.Returns(_studentId);
        _currentUser.IsInRole(Roles.Administrator).Returns(true);

        Result<EnrollmentDto> result =
            await _sut.Handle(new UpdateProgressCommand(enrollment.Id, 50), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task A_dropped_enrollment_rejects_progress_until_it_is_rejoined()
    {
        Enrollment enrollment = Arrange();
        enrollment.Drop();
        _currentUser.UserId.Returns(_studentId);

        Result<EnrollmentDto> result =
            await _sut.Handle(new UpdateProgressCommand(enrollment.Id, 30), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EnrollmentErrors.NotActive);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_missing_enrollment_is_reported_as_not_found()
    {
        _enrollments.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Enrollment?)null);
        _currentUser.UserId.Returns(_studentId);

        Result<EnrollmentDto> result =
            await _sut.Handle(new UpdateProgressCommand(Guid.NewGuid(), 10), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EnrollmentErrors.NotFound);
    }
}
