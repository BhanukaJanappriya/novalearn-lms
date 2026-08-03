using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Domain.Quizzes;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.GetQuizResults;

public sealed class GetQuizResultsQueryHandler(
    IQuizRepository quizzes,
    ICurrentUser currentUser)
    : IRequestHandler<GetQuizResultsQuery, Result<QuizResultsDto>>
{
    public async Task<Result<QuizResultsDto>> Handle(
        GetQuizResultsQuery request, CancellationToken cancellationToken)
    {
        Quiz? quiz = await quizzes.GetQuizWithQuestionsAsync(request.QuizId, cancellationToken);
        if (quiz is null)
        {
            return Result.Failure<QuizResultsDto>(QuizErrors.QuizNotFound);
        }

        if (QuizAuthority.CheckCanManage(quiz.Course, currentUser) is { } denied)
        {
            return Result.Failure<QuizResultsDto>(denied);
        }

        IReadOnlyList<QuizAttempt> attempts =
            await quizzes.ListAttemptsAsync(request.QuizId, cancellationToken);

        List<QuizAttemptSummaryDto> rows = attempts
            .Select(a => new QuizAttemptSummaryDto(
                a.Id,
                a.StudentId,
                a.Student?.FullName ?? "Unknown",
                a.Student?.Email ?? string.Empty,
                a.AttemptNumber,
                a.SubmittedAtUtc,
                a.PointsAwarded,
                a.TotalPoints,
                a.ScorePercent,
                a.IsPassed,
                a.WasLate))
            .ToList();

        return new QuizResultsDto(
            quiz.Id,
            quiz.Title,
            quiz.TotalPoints,
            quiz.PassingScorePercent,
            rows.Count,
            rows.Select(r => r.StudentId).Distinct().Count(),
            rows.Count == 0 ? null : Math.Round(rows.Average(r => r.ScorePercent), 1),
            // Counted per learner, not per attempt, so three passes by one person is still one pass.
            rows.Where(r => r.IsPassed).Select(r => r.StudentId).Distinct().Count(),
            rows);
    }
}
