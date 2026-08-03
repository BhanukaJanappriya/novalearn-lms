using MediatR;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.GetQuizForAuthoring;

/// <summary>
/// The full quiz including correct answers. Restricted to the owning lecturer or an admin;
/// a learner reaching this would see every answer.
/// </summary>
public sealed record GetQuizForAuthoringQuery(Guid QuizId) : IRequest<Result<QuizAuthoringDto>>;
