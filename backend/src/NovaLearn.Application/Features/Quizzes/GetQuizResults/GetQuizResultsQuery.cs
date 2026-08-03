using MediatR;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.GetQuizResults;

/// <summary>How a quiz went across the cohort. Restricted to the owning lecturer or an admin.</summary>
public sealed record GetQuizResultsQuery(Guid QuizId) : IRequest<Result<QuizResultsDto>>;
