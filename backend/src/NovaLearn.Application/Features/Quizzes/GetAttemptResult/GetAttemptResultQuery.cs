using MediatR;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.GetAttemptResult;

/// <summary>
/// A marked attempt. Readable by the learner who sat it, or by staff on that course. Refused
/// while the attempt is still open, since the result view carries the answer key.
/// </summary>
public sealed record GetAttemptResultQuery(Guid AttemptId) : IRequest<Result<AttemptResultDto>>;
