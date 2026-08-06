using FluentAssertions;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Quizzes;
using Xunit;

namespace NovaLearn.Application.UnitTests.Quizzes;

/// <summary>
/// The rule the whole slice exists for: everything a machine can check is marked on submission,
/// and only essays wait for a person.
/// </summary>
public sealed class EssayMarkingTests
{
    private static readonly DateTimeOffset Started = new(2026, 8, 6, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Submitted = Started.AddMinutes(10);
    private static readonly DateTimeOffset Marked = Started.AddDays(1);

    /// <summary>A 30 point quiz: a 10 point multiple choice and a 20 point essay.</summary>
    private static Quiz BuildQuiz(int? passMark = null)
    {
        Quiz quiz = Quiz.Create(
            Guid.NewGuid(), "Mixed", null, null, null, passMark, false, AssessmentStatus.Published);

        Question mcq = quiz.AddQuestion("Pick one", QuestionType.MultipleChoice, 10, 0, null);
        mcq.ReplaceOptions([("Wrong", false), ("Right", true)]);

        quiz.AddQuestion("Discuss the trade offs", QuestionType.Essay, 20, 1, null);

        return quiz;
    }

    private static Question McqOf(Quiz quiz) => quiz.Questions.First(q => q.Type == QuestionType.MultipleChoice);

    private static Question EssayOf(Quiz quiz) => quiz.Questions.First(q => q.Type == QuestionType.Essay);

    private static Guid CorrectOf(Quiz quiz) => McqOf(quiz).Options.Single(o => o.IsCorrect).Id;

    // --- The question itself ------------------------------------------------------------

    [Fact]
    public void An_essay_needs_no_answer_key_to_be_publishable()
    {
        Question essay = Question.Create(Guid.NewGuid(), "Discuss", QuestionType.Essay, 20, 0);

        essay.IsAnswerable().Should().BeTrue();
        essay.RequiresManualMarking.Should().BeTrue();
    }

    [Fact]
    public void An_essay_never_auto_scores_even_with_text()
    {
        Question essay = Question.Create(Guid.NewGuid(), "Discuss", QuestionType.Essay, 20, 0);

        essay.Mark([], "a long and excellent answer").Should().Be(0);
    }

    [Fact]
    public void Marking_guidance_is_kept_for_essays_and_discarded_for_other_types()
    {
        Question essay = Question.Create(
            Guid.NewGuid(), "Discuss", QuestionType.Essay, 20, 0, markingGuidance: "  Look for trade offs.  ");
        essay.MarkingGuidance.Should().Be("Look for trade offs.");

        Question shortAnswer = Question.Create(
            Guid.NewGuid(), "Name it", QuestionType.ShortAnswer, 5, 0, ["Paris"], markingGuidance: "irrelevant");
        shortAnswer.MarkingGuidance.Should().BeNull();
    }

    // --- Submission ---------------------------------------------------------------------

    [Fact]
    public void An_attempt_with_a_written_essay_lands_in_pending_review()
    {
        Quiz quiz = BuildQuiz();
        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);
        attempt.Respond(McqOf(quiz).Id, [CorrectOf(quiz)], null);
        attempt.Respond(EssayOf(quiz).Id, [], "My considered answer.");

        attempt.Submit(quiz, Submitted);

        attempt.Status.Should().Be(AttemptStatus.PendingReview);
        attempt.AwaitingMarkingCount.Should().Be(1);

        // The auto-marked part is already banked, so the score can only go up from here.
        attempt.PointsAwarded.Should().Be(10);
        attempt.ScorePercent.Should().Be(33.3);
    }

    /// <summary>A pass is a statement about a final score, so it must not be claimed early.</summary>
    [Fact]
    public void A_pending_attempt_never_reports_a_pass_even_if_the_provisional_score_clears_the_bar()
    {
        Quiz quiz = BuildQuiz(passMark: 30);
        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);
        attempt.Respond(McqOf(quiz).Id, [CorrectOf(quiz)], null);
        attempt.Respond(EssayOf(quiz).Id, [], "Something");

        attempt.Submit(quiz, Submitted);

        attempt.ScorePercent.Should().Be(33.3, "the provisional score already clears 30");
        attempt.IsPassed.Should().BeFalse("but nothing passes until marking is finished");
    }

    /// <summary>Keeps the marking queue to real work rather than blank pages.</summary>
    [Fact]
    public void An_essay_left_blank_is_scored_zero_rather_than_queued_for_a_person()
    {
        Quiz quiz = BuildQuiz();
        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);
        attempt.Respond(McqOf(quiz).Id, [CorrectOf(quiz)], null);

        attempt.Submit(quiz, Submitted);

        attempt.Status.Should().Be(AttemptStatus.Graded);
        attempt.AwaitingMarkingCount.Should().Be(0);
    }

    [Fact]
    public void A_quiz_with_no_essays_is_graded_outright()
    {
        Quiz quiz = Quiz.Create(
            Guid.NewGuid(), "Auto only", null, null, null, null, false, AssessmentStatus.Published);
        Question mcq = quiz.AddQuestion("Pick", QuestionType.MultipleChoice, 10, 0, null);
        mcq.ReplaceOptions([("A", true), ("B", false)]);

        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);
        attempt.Respond(mcq.Id, [mcq.Options.First(o => o.IsCorrect).Id], null);
        attempt.Submit(quiz, Submitted);

        attempt.Status.Should().Be(AttemptStatus.Graded);
    }

    // --- Manual marking -----------------------------------------------------------------

    [Fact]
    public void Marking_the_last_essay_finalises_the_attempt()
    {
        Quiz quiz = BuildQuiz(passMark: 60);
        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);
        attempt.Respond(McqOf(quiz).Id, [CorrectOf(quiz)], null);
        attempt.Respond(EssayOf(quiz).Id, [], "My considered answer.");
        attempt.Submit(quiz, Submitted);

        Guid answerId = attempt.Answers.Single(a => a.RequiresManualMarking).Id;
        Guid marker = Guid.NewGuid();

        bool marked = attempt.MarkEssay(answerId, 18, "  Well argued.  ", quiz, marker, Marked);

        marked.Should().BeTrue();
        attempt.Status.Should().Be(AttemptStatus.Graded);
        attempt.PointsAwarded.Should().Be(28, "10 auto plus 18 by hand");
        attempt.ScorePercent.Should().Be(93.3);
        attempt.IsPassed.Should().BeTrue();
        attempt.MarkedAtUtc.Should().Be(Marked);

        AttemptAnswer answer = attempt.Answers.Single(a => a.Id == answerId);
        answer.Feedback.Should().Be("Well argued.");
        answer.MarkedById.Should().Be(marker);
        answer.IsManuallyMarked.Should().BeTrue();
    }

    [Fact]
    public void A_marker_cannot_award_more_than_the_essay_is_worth()
    {
        Quiz quiz = BuildQuiz();
        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);
        attempt.Respond(EssayOf(quiz).Id, [], "Answer");
        attempt.Submit(quiz, Submitted);

        Guid answerId = attempt.Answers.Single().Id;
        attempt.MarkEssay(answerId, 500, null, quiz, Guid.NewGuid(), Marked);

        attempt.Answers.Single().PointsAwarded.Should().Be(20);
    }

    [Fact]
    public void An_attempt_stays_pending_until_every_essay_is_marked()
    {
        Quiz quiz = Quiz.Create(
            Guid.NewGuid(), "Two essays", null, null, null, null, false, AssessmentStatus.Published);
        Question first = quiz.AddQuestion("One", QuestionType.Essay, 10, 0, null);
        Question second = quiz.AddQuestion("Two", QuestionType.Essay, 10, 1, null);

        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);
        attempt.Respond(first.Id, [], "A");
        attempt.Respond(second.Id, [], "B");
        attempt.Submit(quiz, Submitted);

        attempt.AwaitingMarkingCount.Should().Be(2);

        Guid firstAnswer = attempt.Answers.Single(a => a.QuestionId == first.Id).Id;
        attempt.MarkEssay(firstAnswer, 8, null, quiz, Guid.NewGuid(), Marked);

        attempt.Status.Should().Be(AttemptStatus.PendingReview);
        attempt.AwaitingMarkingCount.Should().Be(1);

        Guid secondAnswer = attempt.Answers.Single(a => a.QuestionId == second.Id).Id;
        attempt.MarkEssay(secondAnswer, 6, null, quiz, Guid.NewGuid(), Marked);

        attempt.Status.Should().Be(AttemptStatus.Graded);
        attempt.PointsAwarded.Should().Be(14);
    }

    [Fact]
    public void An_auto_marked_answer_cannot_be_marked_by_hand()
    {
        Quiz quiz = BuildQuiz();
        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);
        attempt.Respond(McqOf(quiz).Id, [CorrectOf(quiz)], null);
        attempt.Submit(quiz, Submitted);

        Guid autoAnswerId = attempt.Answers.Single().Id;

        attempt.MarkEssay(autoAnswerId, 10, null, quiz, Guid.NewGuid(), Marked).Should().BeFalse();
    }

    [Fact]
    public void An_attempt_still_in_progress_cannot_be_marked()
    {
        Quiz quiz = BuildQuiz();
        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);
        AttemptAnswer answer = attempt.Respond(EssayOf(quiz).Id, [], "Answer")!;

        attempt.MarkEssay(answer.Id, 10, null, quiz, Guid.NewGuid(), Marked).Should().BeFalse();
    }

    [Fact]
    public void Remarking_an_essay_replaces_the_earlier_mark()
    {
        Quiz quiz = BuildQuiz();
        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);
        attempt.Respond(EssayOf(quiz).Id, [], "Answer");
        attempt.Submit(quiz, Submitted);

        Guid answerId = attempt.Answers.Single().Id;
        attempt.MarkEssay(answerId, 5, "Thin.", quiz, Guid.NewGuid(), Marked);
        attempt.MarkEssay(answerId, 15, "On reflection, better.", quiz, Guid.NewGuid(), Marked);

        attempt.PointsAwarded.Should().Be(15);
        attempt.Answers.Single().Feedback.Should().Be("On reflection, better.");
    }
}
