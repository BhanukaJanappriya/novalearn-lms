using MediatR;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.GetCourseQuizzes;

/// <summary>
/// A course's quizzes. Staff see drafts and publication readiness; enrolled learners see
/// published quizzes with their own attempt history. Neither shape carries question content.
/// </summary>
public sealed record GetCourseQuizzesQuery(Guid CourseId) : IRequest<Result<IReadOnlyList<QuizSummaryDto>>>;
