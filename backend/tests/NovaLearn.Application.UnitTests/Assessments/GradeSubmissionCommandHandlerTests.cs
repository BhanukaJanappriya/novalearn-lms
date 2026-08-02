using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Assessments.Common;
using NovaLearn.Application.Features.Assessments.GradeSubmission;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Assessments;

public sealed class GradeSubmissionCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 2, 10, 0, 0, TimeSpan.Zero);

    private readonly IAssessmentRepository _assessments = Substitute.For<IAssessmentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
    private readonly Guid _lecturerId = Guid.NewGuid();
    private readonly GradeSubmissionCommandHandler _sut;

    public GradeSubmissionCommandHandlerTests()
    {
        _dateTime.UtcNow.Returns(Now);
        _sut = new GradeSubmissionCommandHandler(_assessments, _unitOfWork, _currentUser, _dateTime);
    }

    private Submission Arrange(int maxPoints = 100)
    {
        Course course = Course.Create(
            "Intro", "CS101", null, "Computer Science",
            CourseLevel.Beginner, CourseStatus.Published, 0m, null, _lecturerId);

        Assignment assignment = Assignment.Create(
            course.Id, "Problem set", null, null, maxPoints, false, AssessmentStatus.Published);

        Submission submission = Submission.Create(
            assignment.Id, Guid.NewGuid(), "answer", null, Now.AddDays(-1), false);

        // EF populates these navigations with Include; the tests reach them the same way.
        typeof(Assignment).GetProperty(nameof(Assignment.Course))!.SetValue(assignment, course);
        typeof(Submission).GetProperty(nameof(Submission.Assignment))!.SetValue(submission, assignment);

        _assessments.GetSubmissionAsync(submission.Id, Arg.Any<CancellationToken>()).Returns(submission);
        return submission;
    }

    [Fact]
    public async Task The_owning_lecturer_can_mark_work()
    {
        Submission submission = Arrange();
        _currentUser.UserId.Returns(_lecturerId);

        Result<SubmissionDto> result = await _sut.Handle(
            new GradeSubmissionCommand(submission.Id, 85, "solid"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PointsAwarded.Should().Be(85);
        submission.Status.Should().Be(SubmissionStatus.Graded);
        submission.GradedById.Should().Be(_lecturerId);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Another_lecturer_cannot_mark_work_on_a_course_they_do_not_own()
    {
        Submission submission = Arrange();
        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);

        Result<SubmissionDto> result = await _sut.Handle(
            new GradeSubmissionCommand(submission.Id, 85, null), CancellationToken.None);

        result.Error.Should().Be(AssessmentErrors.NotCourseOwner);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_administrator_may_mark_any_course()
    {
        Submission submission = Arrange();
        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.IsInRole(Roles.Administrator).Returns(true);

        Result<SubmissionDto> result = await _sut.Handle(
            new GradeSubmissionCommand(submission.Id, 70, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// The validator only rejects points above the global ceiling; the per-assignment ceiling is
    /// enforced by the aggregate, which is what stops 500 points on a 20 point task.
    /// </summary>
    [Fact]
    public async Task Points_above_what_the_assignment_is_worth_are_clamped()
    {
        Submission submission = Arrange(maxPoints: 20);
        _currentUser.UserId.Returns(_lecturerId);

        Result<SubmissionDto> result = await _sut.Handle(
            new GradeSubmissionCommand(submission.Id, 500, null), CancellationToken.None);

        result.Value.PointsAwarded.Should().Be(20);
    }

    [Fact]
    public async Task A_missing_submission_is_reported_as_not_found()
    {
        _assessments.GetSubmissionAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Submission?)null);
        _currentUser.UserId.Returns(_lecturerId);

        Result<SubmissionDto> result = await _sut.Handle(
            new GradeSubmissionCommand(Guid.NewGuid(), 10, null), CancellationToken.None);

        result.Error.Should().Be(AssessmentErrors.SubmissionNotFound);
    }
}
