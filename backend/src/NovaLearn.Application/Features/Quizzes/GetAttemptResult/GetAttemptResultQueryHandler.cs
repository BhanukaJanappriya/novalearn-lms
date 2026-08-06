using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Domain.Quizzes;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.GetAttemptResult;

public sealed class GetAttemptResultQueryHandler(
    IQuizRepository quizzes,
    ICurrentUser currentUser)
    : IRequestHandler<GetAttemptResultQuery, Result<AttemptResultDto>>
{
    public async Task<Result<AttemptResultDto>> Handle(
        GetAttemptResultQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId)
        {
            return Result.Failure<AttemptResultDto>(QuizErrors.Unauthenticated);
        }

        QuizAttempt? attempt = await quizzes.GetAttemptAsync(request.AttemptId, cancellationToken);
        if (attempt is null)
        {
            return Result.Failure<AttemptResultDto>(QuizErrors.AttemptNotFound);
        }

        bool isOwner = attempt.StudentId == callerId;
        bool isStaff = QuizAuthority.CheckCanManage(attempt.Quiz?.Course, currentUser) is null;

        if (!isOwner && !isStaff)
        {
            return Result.Failure<AttemptResultDto>(QuizErrors.NotAttemptOwner);
        }

        // This shape carries correct answers, so an attempt still in progress must not return it.
        // PendingReview is fine: it has been handed in, the score is just not final yet.
        if (attempt.Status == AttemptStatus.InProgress)
        {
            return Result.Failure<AttemptResultDto>(QuizErrors.AttemptNotSubmitted);
        }

        Quiz? quiz = await quizzes.GetQuizWithQuestionsAsync(attempt.QuizId, cancellationToken);
        if (quiz is null)
        {
            return Result.Failure<AttemptResultDto>(QuizErrors.QuizNotFound);
        }

        return AttemptResultDto.FromEntity(attempt, quiz);
    }
}
