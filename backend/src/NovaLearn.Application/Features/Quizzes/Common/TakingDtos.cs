using NovaLearn.Domain.Quizzes;

namespace NovaLearn.Application.Features.Quizzes.Common;

/// <summary>
/// A question as the learner sitting the quiz sees it.
///
/// This type deliberately has no <c>IsCorrect</c>, no accepted-answer list and no marking
/// guidance. That omission is the whole security boundary of the quiz feature: anything
/// reachable from here is visible in the browser's network tab, so a correct answer that appears
/// in this shape is a leaked answer. The authoring shape lives separately in
/// <see cref="AuthoringQuestionDto"/>.
/// </summary>
public sealed record TakingQuestionDto(
    Guid Id,
    string Text,
    string Type,
    int Points,
    int SortOrder,
    bool IsRequired,
    bool AllowsMultipleSelections,
    /// <summary>True when the learner writes prose that a person will mark.</summary>
    bool IsEssay,
    IReadOnlyList<TakingOptionDto> Options,
    IReadOnlyList<Guid> SelectedOptionIds,
    string? TextAnswer)
{
    public static TakingQuestionDto FromEntity(Question question, AttemptAnswer? answer) => new(
        question.Id,
        question.Text,
        question.Type.ToString(),
        question.Points,
        question.SortOrder,
        question.IsRequired,
        question.AllowsMultipleSelections,
        question.RequiresManualMarking,
        question.Options
            .OrderBy(o => o.SortOrder)
            .Select(o => new TakingOptionDto(o.Id, o.Text))
            .ToList(),
        answer?.SelectedOptions ?? [],
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
/// A finished attempt. Correctness appears here because the attempt is over, so showing it
/// teaches rather than leaks.
///
/// While <see cref="Status"/> is PendingReview the score is provisional: the essays have not been
/// marked yet, so it can only go up.
/// </summary>
public sealed record AttemptResultDto(
    Guid AttemptId,
    Guid QuizId,
    string QuizTitle,
    Guid StudentId,
    string StudentName,
    int AttemptNumber,
    string Status,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? SubmittedAtUtc,
    DateTimeOffset? MarkedAtUtc,
    int PointsAwarded,
    int TotalPoints,
    double ScorePercent,
    bool IsPassed,
    bool WasLate,
    bool IsAwaitingMarking,
    int AwaitingMarkingCount,
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
                    answer.Id,
                    answer.QuestionId,
                    question?.Text ?? string.Empty,
                    question?.Type.ToString() ?? string.Empty,
                    answer.SelectedOptions,
                    SelectedTexts(question, answer),
                    answer.TextAnswer,
                    CorrectTexts(question),
                    question?.AcceptedAnswerList ?? [],
                    answer.IsCorrect,
                    answer.PointsAwarded,
                    question?.Points ?? 0,
                    answer.RequiresManualMarking,
                    answer.IsManuallyMarked,
                    answer.IsAwaitingMarking,
                    answer.Feedback);
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
            attempt.Status.ToString(),
            attempt.StartedAtUtc,
            attempt.SubmittedAtUtc,
            attempt.MarkedAtUtc,
            attempt.PointsAwarded,
            attempt.TotalPoints,
            attempt.ScorePercent,
            attempt.IsPassed,
            attempt.WasLate,
            attempt.HasPendingManualMarking,
            attempt.AwaitingMarkingCount,
            quiz.PassingScorePercent,
            answers);
    }

    private static IReadOnlyList<string> SelectedTexts(Question? question, AttemptAnswer answer) =>
        question is null
            ? []
            : answer.SelectedOptions
                .Select(id => question.Options.FirstOrDefault(o => o.Id == id)?.Text)
                .Where(text => text is not null)
                .Select(text => text!)
                .ToList();

    private static IReadOnlyList<string> CorrectTexts(Question? question) =>
        question is null
            ? []
            : question.Options.Where(o => o.IsCorrect).Select(o => o.Text).ToList();
}

/// <summary>One marked answer, with what the learner gave and what was right.</summary>
public sealed record AnswerResultDto(
    Guid AnswerId,
    Guid QuestionId,
    string QuestionText,
    string QuestionType,
    IReadOnlyList<Guid> SelectedOptionIds,
    IReadOnlyList<string> SelectedOptionTexts,
    string? TextAnswer,
    IReadOnlyList<string> CorrectOptionTexts,
    IReadOnlyList<string> AcceptedAnswers,
    /// <summary>Null while an essay is still unmarked.</summary>
    bool? IsCorrect,
    int PointsAwarded,
    int PointsPossible,
    bool RequiresManualMarking,
    bool IsManuallyMarked,
    bool IsAwaitingMarking,
    string? Feedback);

/// <summary>One learner's line in the results roster.</summary>
public sealed record QuizAttemptSummaryDto(
    Guid AttemptId,
    Guid StudentId,
    string StudentName,
    string StudentEmail,
    int AttemptNumber,
    string Status,
    DateTimeOffset? SubmittedAtUtc,
    int PointsAwarded,
    int TotalPoints,
    double ScorePercent,
    bool IsPassed,
    bool WasLate,
    bool IsAwaitingMarking,
    int AwaitingMarkingCount);

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
    /// <summary>How many attempts still need a person, so the queue is visible at a glance.</summary>
    int AwaitingReviewCount,
    IReadOnlyList<QuizAttemptSummaryDto> Attempts);
