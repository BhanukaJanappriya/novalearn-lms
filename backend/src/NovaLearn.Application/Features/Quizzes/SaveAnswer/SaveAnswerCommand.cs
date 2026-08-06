using MediatR;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.SaveAnswer;

/// <summary>
/// Records one answer while the attempt is open, so progress survives a reload. Returns nothing
/// beyond success: telling the learner whether they were right would leak the answer key.
/// </summary>
public sealed record SaveAnswerCommand(
    Guid AttemptId,
    Guid QuestionId,
    IReadOnlyList<Guid> SelectedOptionIds,
    string? TextAnswer) : IRequest<Result>;
