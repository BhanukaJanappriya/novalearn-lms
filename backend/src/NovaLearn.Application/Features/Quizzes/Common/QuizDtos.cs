using NovaLearn.Domain.Quizzes;

namespace NovaLearn.Application.Features.Quizzes.Common;

/// <summary>
/// A quiz in a list. Safe for either audience: it carries no question content at all, so there
/// is nothing here a learner could mine for answers.
/// </summary>
public sealed record QuizSummaryDto(
    Guid Id,
    Guid CourseId,
    string Title,
    string? Description,
    string Status,
    int? TimeLimitMinutes,
    int? MaxAttempts,
    int? PassingScorePercent,
    bool ShuffleQuestions,
    int QuestionCount,
    int TotalPoints,
    bool IsReadyToPublish,
    bool HasManuallyMarkedQuestions,
    int AttemptsUsed,
    double? BestScorePercent,
    bool HasPassed,
    bool CanAttempt)
{
    /// <summary>Staff projection. Attempt fields are neutral, since staff do not sit the quiz.</summary>
    public static QuizSummaryDto ForStaff(Quiz quiz) => new(
        quiz.Id,
        quiz.CourseId,
        quiz.Title,
        quiz.Description,
        quiz.Status.ToString(),
        quiz.TimeLimitMinutes,
        quiz.MaxAttempts,
        quiz.PassingScorePercent,
        quiz.ShuffleQuestions,
        quiz.Questions.Count,
        quiz.TotalPoints,
        quiz.IsReadyToPublish(),
        quiz.HasManuallyMarkedQuestions,
        AttemptsUsed: 0,
        BestScorePercent: null,
        HasPassed: false,
        CanAttempt: false);

    /// <summary>
    /// Learner projection, carrying their own standing. <c>IsReadyToPublish</c> is an authoring
    /// concern, so it is reported as true and never shown.
    /// </summary>
    public static QuizSummaryDto ForLearner(Quiz quiz, IReadOnlyList<QuizAttempt> mine) => new(
        quiz.Id,
        quiz.CourseId,
        quiz.Title,
        quiz.Description,
        quiz.Status.ToString(),
        quiz.TimeLimitMinutes,
        quiz.MaxAttempts,
        quiz.PassingScorePercent,
        quiz.ShuffleQuestions,
        quiz.Questions.Count,
        quiz.TotalPoints,
        IsReadyToPublish: true,
        quiz.HasManuallyMarkedQuestions,
        AttemptsUsed: mine.Count,
        BestScorePercent: mine.Count == 0 ? null : mine.Max(a => a.ScorePercent),
        HasPassed: mine.Any(a => a.IsPassed),
        CanAttempt: quiz.AllowsAnotherAttempt(mine.Count));
}

/// <summary>
/// The authoring view of a question: includes which option is correct and the accepted answers.
/// Never returned to a learner. See <see cref="TakingQuestionDto"/> for their view.
/// </summary>
public sealed record AuthoringQuestionDto(
    Guid Id,
    string Text,
    string Type,
    int Points,
    int SortOrder,
    bool IsRequired,
    string? MarkingGuidance,
    bool RequiresManualMarking,
    bool AllowsMultipleSelections,
    IReadOnlyList<string> AcceptedAnswers,
    IReadOnlyList<AuthoringOptionDto> Options,
    bool IsAnswerable)
{
    public static AuthoringQuestionDto FromEntity(Question question) => new(
        question.Id,
        question.Text,
        question.Type.ToString(),
        question.Points,
        question.SortOrder,
        question.IsRequired,
        question.MarkingGuidance,
        question.RequiresManualMarking,
        question.AllowsMultipleSelections,
        question.AcceptedAnswerList,
        question.Options
            .OrderBy(o => o.SortOrder)
            .Select(o => new AuthoringOptionDto(o.Id, o.Text, o.IsCorrect, o.SortOrder))
            .ToList(),
        question.IsAnswerable());
}

public sealed record AuthoringOptionDto(Guid Id, string Text, bool IsCorrect, int SortOrder);

/// <summary>The full quiz as its author sees it, answers included.</summary>
public sealed record QuizAuthoringDto(
    QuizSummaryDto Quiz,
    IReadOnlyList<AuthoringQuestionDto> Questions)
{
    public static QuizAuthoringDto FromEntity(Quiz quiz) => new(
        QuizSummaryDto.ForStaff(quiz),
        quiz.Questions
            .OrderBy(q => q.SortOrder)
            .Select(AuthoringQuestionDto.FromEntity)
            .ToList());
}
