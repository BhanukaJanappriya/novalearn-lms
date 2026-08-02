using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Assessments.Common;
using NovaLearn.Application.Features.Assessments.SubmitAssignment;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Assessments;

public sealed class SubmitAssignmentCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 2, 10, 0, 0, TimeSpan.Zero);

    private readonly IAssessmentRepository _assessments = Substitute.For<IAssessmentRepository>();
    private readonly IEnrollmentRepository _enrollments = Substitute.For<IEnrollmentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
    private readonly Guid _studentId = Guid.NewGuid();
    private readonly Guid _courseId = Guid.NewGuid();
    private readonly SubmitAssignmentCommandHandler _sut;

    public SubmitAssignmentCommandHandlerTests()
    {
        _dateTime.UtcNow.Returns(Now);
        _currentUser.UserId.Returns(_studentId);
        _sut = new SubmitAssignmentCommandHandler(
            _assessments, _enrollments, _unitOfWork, _currentUser, _dateTime);
    }

    private Assignment Arrange(
        DateTimeOffset? due = null,
        bool allowLate = false,
        AssessmentStatus status = AssessmentStatus.Published,
        bool enrolled = true)
    {
        Assignment assignment = Assignment.Create(_courseId, "Problem set", null, due, 100, allowLate, status);
        _assessments.GetAssignmentAsync(assignment.Id, Arg.Any<CancellationToken>()).Returns(assignment);

        _enrollments.GetActiveAsync(_studentId, _courseId, Arg.Any<CancellationToken>())
            .Returns(enrolled ? Enrollment.Create(_studentId, _courseId, Now.AddDays(-10)) : null);

        // The handler re-reads after saving so the DTO projects fully.
        _assessments.GetSubmissionAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(call => AttachedSubmission(assignment, call.Arg<Guid>()));

        return assignment;
    }

    private Submission AttachedSubmission(Assignment assignment, Guid id)
    {
        Submission submission = Submission.Create(assignment.Id, _studentId, "answer", null, Now, false);
        typeof(Submission).GetProperty(nameof(Submission.Assignment))!.SetValue(submission, assignment);
        typeof(NovaLearn.Domain.Common.BaseEntity).GetProperty("Id")!.SetValue(submission, id);
        return submission;
    }

    private static SubmitAssignmentCommand CommandFor(Guid assignmentId) =>
        new(assignmentId, "my answer", null);

    [Fact]
    public async Task An_enrolled_learner_can_hand_work_in()
    {
        Assignment assignment = Arrange();

        Result<SubmissionDto> result = await _sut.Handle(CommandFor(assignment.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _assessments.Received(1).AddSubmissionAsync(
            Arg.Any<Submission>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_learner_who_is_not_enrolled_is_refused()
    {
        Assignment assignment = Arrange(enrolled: false);

        Result<SubmissionDto> result = await _sut.Handle(CommandFor(assignment.Id), CancellationToken.None);

        result.Error.Should().Be(AssessmentErrors.NotEnrolled);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_draft_assignment_cannot_be_submitted_to()
    {
        Assignment assignment = Arrange(status: AssessmentStatus.Draft);

        Result<SubmissionDto> result = await _sut.Handle(CommandFor(assignment.Id), CancellationToken.None);

        result.Error.Should().Be(AssessmentErrors.AssignmentNotPublished);
    }

    [Fact]
    public async Task Work_is_refused_once_the_due_date_has_passed()
    {
        Assignment assignment = Arrange(due: Now.AddHours(-1), allowLate: false);

        Result<SubmissionDto> result = await _sut.Handle(CommandFor(assignment.Id), CancellationToken.None);

        result.Error.Should().Be(AssessmentErrors.NotOpen);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Late_work_is_accepted_and_flagged_when_the_assignment_allows_it()
    {
        Assignment assignment = Arrange(due: Now.AddHours(-1), allowLate: true);

        Result<SubmissionDto> result = await _sut.Handle(CommandFor(assignment.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _assessments.Received(1).AddSubmissionAsync(
            Arg.Is<Submission>(s => s.IsLate), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Submitting_again_replaces_the_existing_attempt_rather_than_adding_one()
    {
        Assignment assignment = Arrange();
        Submission existing = Submission.Create(assignment.Id, _studentId, "first try", null, Now.AddDays(-1), false);
        existing.Grade(90, "great", 100, Guid.NewGuid(), Now.AddHours(-2));

        _assessments.GetSubmissionForStudentAsync(assignment.Id, _studentId, Arg.Any<CancellationToken>())
            .Returns(existing);

        Result<SubmissionDto> result = await _sut.Handle(CommandFor(assignment.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _assessments.DidNotReceive().AddSubmissionAsync(
            Arg.Any<Submission>(), Arg.Any<CancellationToken>());

        // The replaced work must not keep the mark it earned.
        existing.Content.Should().Be("my answer");
        existing.Status.Should().Be(SubmissionStatus.Submitted);
        existing.PointsAwarded.Should().BeNull();
    }

    [Fact]
    public async Task A_missing_assignment_is_reported_as_not_found()
    {
        _assessments.GetAssignmentAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Assignment?)null);

        Result<SubmissionDto> result = await _sut.Handle(CommandFor(Guid.NewGuid()), CancellationToken.None);

        result.Error.Should().Be(AssessmentErrors.AssignmentNotFound);
    }
}
