using MediatR;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Domain.Quizzes;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.SaveQuestion;

/// <summary>
/// Creates or replaces a question wholesale. One command for both because a question and its
/// options only make sense together: editing them separately would let a question sit with no
/// correct answer, which scoring cannot resolve.
/// </summary>
public sealed record SaveQuestionCommand(
    Guid QuizId,
    /// <summary>Null creates a question; an id replaces that one.</summary>
    Guid? QuestionId,
    string Text,
    QuestionType Type,
    int Points,
    IReadOnlyList<string> AcceptedAnswers,
    IReadOnlyList<QuestionOptionInput> Options,
    bool IsRequired,
    string? MarkingGuidance) : IRequest<Result<AuthoringQuestionDto>>;

/// <summary>One option as submitted by the author.</summary>
public sealed record QuestionOptionInput(string Text, bool IsCorrect);
