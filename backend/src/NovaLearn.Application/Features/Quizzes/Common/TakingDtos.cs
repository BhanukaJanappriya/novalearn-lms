using NovaLearn.Domain.Quizzes;

namespace NovaLearn.Application.Features.Quizzes.Common;

/// <summary>
/// A question as the learner sitting the quiz sees it.
///
/// This type deliberately has no <c>IsCorrect</c> and no accepted-answer list. That omission is
/// the whole security boundary of the quiz feature: anything reachable from here is visible in
/// the browser's network tab, so a correct answer that appears in this shape is a leaked answer.
/// The authoring shape lives separately in <see cref="AuthoringQuestionDto"/>.
/// </summary>
public sealed record TakingQuestionDto(
    Guid Id,
    string Text,
    string Type,
    int Points,
    int SortOrder,
    IReadOnlyList<TakingOptionDto> Options,
    Guid? SelectedOptionId,
    string? TextAnswer)
{
    public static TakingQuestionDto FromEntity(Question question, AttemptAnswer? answer) => new(
        question.Id,
        question.Text,
        question.Type.ToString(),
        question.Points,
        question.SortOrder,
        question.Options
            .OrderBy(o => o.SortOrder)
            .Select(o => new TakingOptionDto(o.Id, o.Text))
            .ToList(),
        answer?.SelectedOptionId,
        answer?.TextAnswer);
}

/// <summary>An option with its text only. Correctness is deliberately absent.</summary>
public sealed record TakingOptionDto(Guid Id, string Text);

/// <summary>An attempt in progress: the questions to answer and how long is left.</summary>
public sealed record AttemptInProgressDto(
    Guid AttemptId,
    Guid QuizId,
    string QuizTitle,
    int AttemptNumber,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? DeadlineUtc,
    int TotalPoints,
    IReadOnlyList<TakingQuestionDto> Questions);

/// <summary>
/// A finished attempt. Correctness appears here because the attempt is over and marked, so
/// showing it teaches rather than leaks.
/// </summary>
public sealed record AttemptResultDto(
    Guid AttemptId,
    Guid QuizId,
    string QuizTitle,
    Guid StudentId,
    string StudentName,
    int AttemptNumber,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? SubmittedAtUtc,
    int PointsAwarded,
    int TotalPoints,
    double ScorePercent,
    bool IsPassed,
    bool WasLate,
    int? PassingScorePercent,
    IReadOnlyList<AnswerResultDto> Answers)
{
    public static AttemptResultDto FromEntity(QuizAttempt attempt, Quiz quiz)
    {
        Dictionary<Guid, Question> questions = quiz.Questions.ToDictionary(q => q.Id);

        List<AnswerResultDto> answers = attempt.Answers
            .Select(answer =>
            {
                questions.TryGetValue(answer.QuestionId, out Question? question);

                return new AnswerResultDto(
                    answer.QuestionId,
                    question?.Text ?? string.Empty,
                    question?.Type.ToString() ?? string.Empty,
                    answer.SelectedOptionId,
                    question?.Options.FirstOrDefault(o => o.Id == answer.SelectedOptionId)?.Text,
                    answer.TextAnswer,
                    question?.Options.FirstOrDefault(o => o.IsCorrect)?.Text,
                    question?.AcceptedAnswerList ?? [],
                    answer.IsCorrect ?? false,
                    answer.PointsAwarded,
                    question?.Points ?? 0);
            })
            .OrderBy(a => questions.TryGetValue(a.QuestionId, out Question? q) ? q.SortOrder : 0)
            .ToList();

        return new AttemptResultDto(
            attempt.Id,
            attempt.QuizId,
            quiz.Title,
            attempt.StudentId,
            attempt.Student?.FullName ?? "Unknown",
            attempt.AttemptNumber,
            attempt.StartedAtUtc,
            attempt.SubmittedAtUtc,
            attempt.PointsAwarded,
            attempt.TotalPoints,
            attempt.ScorePercent,
            attempt.IsPassed,
            attempt.WasLate,
            quiz.PassingScorePercent,
            answers);
    }
}

/// <summary>One marked answer, with what the learner gave and what was right.</summary>
public sealed record AnswerResultDto(
    Guid QuestionId,
    string QuestionText,
    string QuestionType,
    Guid? SelectedOptionId,
    string? SelectedOptionText,
    string? TextAnswer,
    string? CorrectOptionText,
    IReadOnlyList<string> AcceptedAnswers,
    bool IsCorrect,
    int PointsAwarded,
    int PointsPossible);

/// <summary>One learner's line in the results roster.</summary>
public sealed record QuizAttemptSummaryDto(
    Guid AttemptId,
    Guid StudentId,
    string StudentName,
    string StudentEmail,
    int AttemptNumber,
    DateTimeOffset? SubmittedAtUtc,
    int PointsAwarded,
    int TotalPoints,
    double ScorePercent,
    bool IsPassed,
    bool WasLate);

/// <summary>The staff view of how a quiz went across the cohort.</summary>
public sealed record QuizResultsDto(
    Guid QuizId,
    string QuizTitle,
    int TotalPoints,
    int? PassingScorePercent,
    int AttemptCount,
    int DistinctLearners,
    double? AverageScorePercent,
    int PassedCount,
    IReadOnlyList<QuizAttemptSummaryDto> Attempts);
