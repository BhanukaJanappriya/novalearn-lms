using MediatR;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Domain.Assessments;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.CreateQuiz;

public sealed record CreateQuizCommand(
    Guid CourseId,
    string Title,
    string? Description,
    int? TimeLimitMinutes,
    int? MaxAttempts,
    int? PassingScorePercent,
    bool ShuffleQuestions,
    AssessmentStatus Status) : IRequest<Result<QuizSummaryDto>>;
