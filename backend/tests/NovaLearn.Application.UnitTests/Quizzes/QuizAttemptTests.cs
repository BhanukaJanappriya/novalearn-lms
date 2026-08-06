using FluentAssertions;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Quizzes;
using Xunit;

namespace NovaLearn.Application.UnitTests.Quizzes;

public sealed class QuizAttemptTests
{
    private static readonly DateTimeOffset Started = new(2026, 8, 3, 10, 0, 0, TimeSpan.Zero);

    /// <summary>A two question quiz worth 30: one multiple choice at 10, one short answer at 20.</summary>
    private static Quiz BuildQuiz(int? timeLimit = null, int? passMark = null, int? maxAttempts = null)
    {
        Quiz quiz = Quiz.Create(
            Guid.NewGuid(), "Week 1 check", null, timeLimit, maxAttempts, passMark,
            shuffleQuestions: false, AssessmentStatus.Published);

        Question mcq = quiz.AddQuestion("Pick one", QuestionType.MultipleChoice, 10, 0, null);
        mcq.ReplaceOptions([("Wrong", false), ("Right", true)]);

        quiz.AddQuestion("Capital of France", QuestionType.ShortAnswer, 20, 1, ["Paris"]);

        return quiz;
    }

    private static Guid CorrectOptionOf(Quiz quiz) =>
        quiz.Questions.First(q => q.Type == QuestionType.MultipleChoice)
            .Options.Single(o => o.IsCorrect).Id;

    private static Guid WrongOptionOf(Quiz quiz) =>
        quiz.Questions.First(q => q.Type == QuestionType.MultipleChoice)
            .Options.First(o => !o.IsCorrect).Id;

    private static Question ShortAnswerOf(Quiz quiz) =>
        quiz.Questions.First(q => q.Type == QuestionType.ShortAnswer);

    private static Question MultipleChoiceOf(Quiz quiz) =>
        quiz.Questions.First(q => q.Type == QuestionType.MultipleChoice);

    [Fact]
    public void A_perfect_attempt_scores_one_hundred_percent()
    {
        Quiz quiz = BuildQuiz();
        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);

        attempt.Respond(MultipleChoiceOf(quiz).Id, [CorrectOptionOf(quiz)], null);
        attempt.Respond(ShortAnswerOf(quiz).Id, [], "paris");

        attempt.Submit(quiz, Started.AddMinutes(5));

        attempt.PointsAwarded.Should().Be(30);
        attempt.TotalPoints.Should().Be(30);
        attempt.ScorePercent.Should().Be(100);
        attempt.Status.Should().Be(AttemptStatus.Graded);
    }

    [Fact]
    public void A_partly_correct_attempt_scores_proportionally()
    {
        Quiz quiz = BuildQuiz();
        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);

        attempt.Respond(MultipleChoiceOf(quiz).Id, [WrongOptionOf(quiz)], null);
        attempt.Respond(ShortAnswerOf(quiz).Id, [], "Paris");

        attempt.Submit(quiz, Started.AddMinutes(5));

        // 20 of 30.
        attempt.PointsAwarded.Should().Be(20);
        attempt.ScorePercent.Should().Be(66.7);
    }

    [Fact]
    public void Unanswered_questions_simply_score_zero()
    {
        Quiz quiz = BuildQuiz();
        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);

        attempt.Submit(quiz, Started.AddMinutes(1));

        attempt.PointsAwarded.Should().Be(0);
        attempt.ScorePercent.Should().Be(0);
        attempt.Status.Should().Be(AttemptStatus.Graded);
    }

    [Fact]
    public void Answering_the_same_question_twice_replaces_the_response()
    {
        Quiz quiz = BuildQuiz();
        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);
        Guid questionId = ShortAnswerOf(quiz).Id;

        attempt.Respond(questionId, [], "Lyon");
        attempt.Respond(questionId, [], "Paris");

        attempt.Answers.Should().ContainSingle();
        attempt.Submit(quiz, Started.AddMinutes(2));
        attempt.PointsAwarded.Should().Be(20);
    }

    [Fact]
    public void A_submitted_attempt_refuses_further_answers()
    {
        Quiz quiz = BuildQuiz();
        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);
        attempt.Submit(quiz, Started.AddMinutes(1));

        attempt.Respond(ShortAnswerOf(quiz).Id, [], "Paris").Should().BeNull();
        attempt.PointsAwarded.Should().Be(0, "the late answer must not count");
    }

    [Fact]
    public void Submitting_twice_does_not_rescore()
    {
        Quiz quiz = BuildQuiz();
        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);
        attempt.Respond(ShortAnswerOf(quiz).Id, [], "Paris");
        attempt.Submit(quiz, Started.AddMinutes(1));

        DateTimeOffset firstSubmission = attempt.SubmittedAtUtc!.Value;
        attempt.Submit(quiz, Started.AddMinutes(90));

        attempt.SubmittedAtUtc.Should().Be(firstSubmission);
    }

    [Fact]
    public void The_pass_mark_decides_whether_an_attempt_passed()
    {
        Quiz quiz = BuildQuiz(passMark: 60);
        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);
        attempt.Respond(ShortAnswerOf(quiz).Id, [], "Paris");

        attempt.Submit(quiz, Started.AddMinutes(1));

        attempt.ScorePercent.Should().Be(66.7);
        attempt.IsPassed.Should().BeTrue();
    }

    [Fact]
    public void A_quiz_with_no_pass_mark_never_reports_a_pass()
    {
        Quiz quiz = BuildQuiz(passMark: null);
        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);
        attempt.Respond(MultipleChoiceOf(quiz).Id, [CorrectOptionOf(quiz)], null);
        attempt.Respond(ShortAnswerOf(quiz).Id, [], "Paris");

        attempt.Submit(quiz, Started.AddMinutes(1));

        attempt.ScorePercent.Should().Be(100);
        attempt.IsPassed.Should().BeFalse();
    }

    [Fact]
    public void An_attempt_handed_in_after_the_time_limit_is_flagged()
    {
        Quiz quiz = BuildQuiz(timeLimit: 30);
        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);

        attempt.Submit(quiz, Started.AddMinutes(31));

        attempt.WasLate.Should().BeTrue();
    }

    [Fact]
    public void An_attempt_inside_the_time_limit_is_not_flagged()
    {
        Quiz quiz = BuildQuiz(timeLimit: 30);
        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);

        attempt.Submit(quiz, Started.AddMinutes(29));

        attempt.WasLate.Should().BeFalse();
    }

    [Fact]
    public void An_untimed_quiz_is_never_late()
    {
        Quiz quiz = BuildQuiz(timeLimit: null);
        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, Guid.NewGuid(), 1, Started);

        attempt.Submit(quiz, Started.AddYears(1));

        attempt.WasLate.Should().BeFalse();
    }

    [Fact]
    public void Attempt_limits_are_respected_and_unlimited_when_unset()
    {
        BuildQuiz(maxAttempts: 2).AllowsAnotherAttempt(1).Should().BeTrue();
        BuildQuiz(maxAttempts: 2).AllowsAnotherAttempt(2).Should().BeFalse();
        BuildQuiz(maxAttempts: null).AllowsAnotherAttempt(500).Should().BeTrue();
    }

    [Fact]
    public void A_quiz_is_only_ready_to_publish_once_every_question_is_answerable()
    {
        Quiz empty = Quiz.Create(
            Guid.NewGuid(), "Empty", null, null, null, null, false, AssessmentStatus.Draft);
        empty.IsReadyToPublish().Should().BeFalse();

        Question broken = empty.AddQuestion("No options", QuestionType.MultipleChoice, 5, 0, null);
        empty.IsReadyToPublish().Should().BeFalse();

        broken.ReplaceOptions([("A", true), ("B", false)]);
        empty.IsReadyToPublish().Should().BeTrue();
    }
}
