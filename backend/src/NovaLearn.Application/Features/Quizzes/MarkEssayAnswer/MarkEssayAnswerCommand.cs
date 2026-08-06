using MediatR;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.MarkEssayAnswer;

/// <summary>
/// Records a person's mark on one essay answer. Once the last essay in an attempt is marked, the
/// attempt finalises and its provisional score becomes the real one.
/// </summary>
public sealed record MarkEssayAnswerCommand(
    Guid AttemptId,
    Guid AnswerId,
    int PointsAwarded,
    string? Feedback) : IRequest<Result<AttemptResultDto>>;
