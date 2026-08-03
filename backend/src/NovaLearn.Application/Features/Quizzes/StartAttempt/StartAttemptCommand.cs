using MediatR;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.StartAttempt;

/// <summary>
/// Begins a sitting, or resumes the one already open. Returns the questions to answer, without
/// any correct answers in the payload.
/// </summary>
public sealed record StartAttemptCommand(Guid QuizId) : IRequest<Result<AttemptInProgressDto>>;
