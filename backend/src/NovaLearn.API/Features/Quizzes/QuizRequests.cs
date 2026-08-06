using NovaLearn.Application.Features.Quizzes.SaveQuestion;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Quizzes;

namespace NovaLearn.API.Features.Quizzes;

/// <summary>Body for creating a quiz. Enums accept their string names, e.g. "Published".</summary>
public sealed record CreateQuizRequest(
    string Title,
    string? Description,
    int? TimeLimitMinutes,
    int? MaxAttempts,
    int? PassingScorePercent,
    bool ShuffleQuestions,
    AssessmentStatus Status);

/// <summary>Body for editing a quiz. Replaces every field.</summary>
public sealed record UpdateQuizRequest(
    string Title,
    string? Description,
    int? TimeLimitMinutes,
    int? MaxAttempts,
    int? PassingScorePercent,
    bool ShuffleQuestions,
    AssessmentStatus Status);

/// <summary>
/// Body for creating or replacing a question. Omit <c>QuestionId</c> to create one.
/// Options are replaced wholesale rather than patched.
/// </summary>
public sealed record SaveQuestionRequest(
    Guid? QuestionId,
    string Text,
    QuestionType Type,
    int Points,
    IReadOnlyList<string>? AcceptedAnswers,
    IReadOnlyList<QuestionOptionInput>? Options,
    bool IsRequired = false,
    string? MarkingGuidance = null);

/// <summary>Body for reordering a quiz's questions. Must list every question id, in order.</summary>
public sealed record ReorderQuestionsRequest(IReadOnlyList<Guid> QuestionIds);

/// <summary>Body for a person marking one essay answer.</summary>
public sealed record MarkEssayAnswerRequest(int PointsAwarded, string? Feedback);

/// <summary>Body for recording one answer while an attempt is open.</summary>
public sealed record SaveAnswerRequest(
    Guid QuestionId,
    IReadOnlyList<Guid>? SelectedOptionIds,
    string? TextAnswer);
