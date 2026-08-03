using MediatR;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.SubmitAttempt;

/// <summary>
/// Hands the attempt in and marks it. The result comes back with correct answers included,
/// which is safe because the attempt is now closed.
/// </summary>
public sealed record SubmitAttemptCommand(Guid AttemptId) : IRequest<Result<AttemptResultDto>>;
