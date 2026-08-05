using FluentAssertions;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Assessments.Events;
using NovaLearn.Domain.Common;
using NovaLearn.Domain.Quizzes;
using NovaLearn.Domain.Quizzes.Events;
using Xunit;

namespace NovaLearn.Application.UnitTests.Notifications;

/// <summary>
/// Publication must announce itself exactly once. Raising on every save would notify the whole
/// cohort each time a typo was fixed, which is the fastest way to make people mute an LMS.
/// </summary>
public sealed class PublicationEventTests
{
    private static Assignment Draft() =>
        Assignment.Create(Guid.NewGuid(), "Task", null, null, 10, false, AssessmentStatus.Draft);

    private static Quiz DraftQuiz()
    {
        Quiz quiz = Quiz.Create(
            Guid.NewGuid(), "Check", null, null, null, null, false, AssessmentStatus.Draft);

        Question question = quiz.AddQuestion("Q", QuestionType.MultipleChoice, 5, 0, null);
        question.ReplaceOptions([("A", true), ("B", false)]);

        return quiz;
    }

    private static int CountOf<TEvent>(IHasDomainEvents entity) =>
        entity.DomainEvents.OfType<TEvent>().Count();

    [Fact]
    public void A_draft_assignment_announces_nothing()
    {
        CountOf<AssignmentPublishedDomainEvent>(Draft()).Should().Be(0);
    }

    [Fact]
    public void An_assignment_created_straight_into_published_announces_once()
    {
        Assignment assignment = Assignment.Create(
            Guid.NewGuid(), "Task", null, null, 10, false, AssessmentStatus.Published);

        CountOf<AssignmentPublishedDomainEvent>(assignment).Should().Be(1);
    }

    [Fact]
    public void Publishing_a_draft_assignment_announces_once()
    {
        Assignment assignment = Draft();

        assignment.Update("Task", null, null, 10, false, AssessmentStatus.Published);

        CountOf<AssignmentPublishedDomainEvent>(assignment).Should().Be(1);
    }

    /// <summary>The regression this whole design exists to prevent.</summary>
    [Fact]
    public void Editing_an_already_published_assignment_announces_nothing_further()
    {
        Assignment assignment = Draft();
        assignment.Update("Task", null, null, 10, false, AssessmentStatus.Published);
        assignment.ClearDomainEvents();

        assignment.Update("Task renamed", "new instructions", null, 20, true, AssessmentStatus.Published);

        CountOf<AssignmentPublishedDomainEvent>(assignment).Should().Be(0);
    }

    [Fact]
    public void Unpublishing_then_republishing_announces_again()
    {
        Assignment assignment = Draft();
        assignment.Publish();
        assignment.ClearDomainEvents();

        assignment.Unpublish();
        assignment.Publish();

        CountOf<AssignmentPublishedDomainEvent>(assignment).Should().Be(1);
    }

    [Fact]
    public void Publishing_a_quiz_announces_once_with_its_question_count()
    {
        Quiz quiz = DraftQuiz();

        quiz.Update("Check", null, null, null, null, false, AssessmentStatus.Published);

        QuizPublishedDomainEvent published = quiz.DomainEvents
            .OfType<QuizPublishedDomainEvent>()
            .Should().ContainSingle().Subject;

        published.QuestionCount.Should().Be(1);
        published.Title.Should().Be("Check");
    }

    [Fact]
    public void Editing_a_live_quiz_announces_nothing_further()
    {
        Quiz quiz = DraftQuiz();
        quiz.Publish();
        quiz.ClearDomainEvents();

        quiz.Update("Check renamed", "desc", 30, 2, 50, true, AssessmentStatus.Published);

        CountOf<QuizPublishedDomainEvent>(quiz).Should().Be(0);
    }

    [Fact]
    public void Handing_work_in_announces_it_for_marking()
    {
        Submission submission = Submission.Create(
            Guid.NewGuid(), Guid.NewGuid(), "answer", null, DateTimeOffset.UtcNow, isLate: true);

        SubmissionReceivedDomainEvent received = submission.DomainEvents
            .OfType<SubmissionReceivedDomainEvent>()
            .Should().ContainSingle().Subject;

        received.IsLate.Should().BeTrue();
    }

    [Fact]
    public void Replacing_work_announces_it_for_marking_again()
    {
        Submission submission = Submission.Create(
            Guid.NewGuid(), Guid.NewGuid(), "answer", null, DateTimeOffset.UtcNow, false);
        submission.ClearDomainEvents();

        submission.Resubmit("better answer", null, DateTimeOffset.UtcNow, false);

        CountOf<SubmissionReceivedDomainEvent>(submission).Should().Be(1);
    }

    [Fact]
    public void Marking_work_announces_the_result_with_the_clamped_score()
    {
        Submission submission = Submission.Create(
            Guid.NewGuid(), Guid.NewGuid(), "answer", null, DateTimeOffset.UtcNow, false);
        submission.ClearDomainEvents();

        submission.Grade(500, "great", 20, Guid.NewGuid(), DateTimeOffset.UtcNow);

        SubmissionGradedDomainEvent graded = submission.DomainEvents
            .OfType<SubmissionGradedDomainEvent>()
            .Should().ContainSingle().Subject;

        graded.PointsAwarded.Should().Be(20, "the event must carry the awarded score, not the requested one");
        graded.MaxPoints.Should().Be(20);
    }
}
