using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Domain.Quizzes;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.SubmitAttempt;

public sealed class SubmitAttemptCommandHandler(
    IQuizRepository quizzes,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider dateTime)
    : IRequestHandler<SubmitAttemptCommand, Result<AttemptResultDto>>
{
    public async Task<Result<AttemptResultDto>> Handle(
        SubmitAttemptCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } studentId)
        {
            return Result.Failure<AttemptResultDto>(QuizErrors.Unauthenticated);
        }

        QuizAttempt? attempt = await quizzes.GetAttemptAsync(request.AttemptId, cancellationToken);
        if (attempt is null)
        {
            return Result.Failure<AttemptResultDto>(QuizErrors.AttemptNotFound);
        }

        if (attempt.StudentId != studentId)
        {
            return Result.Failure<AttemptResultDto>(QuizErrors.NotAttemptOwner);
        }

        if (attempt.Status == AttemptStatus.Submitted)
        {
            return Result.Failure<AttemptResultDto>(QuizErrors.AttemptAlreadySubmitted);
        }

        // Questions and their options have to be loaded: each question marks its own answer.
        Quiz? quiz = await quizzes.GetQuizWithQuestionsAsync(attempt.QuizId, cancellationToken);
        if (quiz is null)
        {
            return Result.Failure<AttemptResultDto>(QuizErrors.QuizNotFound);
        }

        // Marking, the score and the pass decision all happen inside the aggregate, so a score
        // can never be set from outside.
        attempt.Submit(quiz, dateTime.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AttemptResultDto.FromEntity(attempt, quiz);
    }
}
