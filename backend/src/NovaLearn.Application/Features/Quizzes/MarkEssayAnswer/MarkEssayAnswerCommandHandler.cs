using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Domain.Quizzes;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.MarkEssayAnswer;

public sealed class MarkEssayAnswerCommandHandler(
    IQuizRepository quizzes,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider dateTime)
    : IRequestHandler<MarkEssayAnswerCommand, Result<AttemptResultDto>>
{
    public async Task<Result<AttemptResultDto>> Handle(
        MarkEssayAnswerCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } markerId)
        {
            return Result.Failure<AttemptResultDto>(QuizErrors.Unauthenticated);
        }

        QuizAttempt? attempt = await quizzes.GetAttemptAsync(request.AttemptId, cancellationToken);
        if (attempt is null)
        {
            return Result.Failure<AttemptResultDto>(QuizErrors.AttemptNotFound);
        }

        // Marking is authoring work: the owning lecturer or an admin, never the learner.
        if (QuizAuthority.CheckCanManage(attempt.Quiz?.Course, currentUser) is { } denied)
        {
            return Result.Failure<AttemptResultDto>(denied);
        }

        // Questions and their points have to be loaded: the aggregate clamps against them.
        Quiz? quiz = await quizzes.GetQuizWithQuestionsAsync(attempt.QuizId, cancellationToken);
        if (quiz is null)
        {
            return Result.Failure<AttemptResultDto>(QuizErrors.QuizNotFound);
        }

        bool marked = attempt.MarkEssay(
            request.AnswerId, request.PointsAwarded, request.Feedback, quiz, markerId, dateTime.UtcNow);

        if (!marked)
        {
            return Result.Failure<AttemptResultDto>(QuizErrors.AnswerNotMarkable);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AttemptResultDto.FromEntity(attempt, quiz);
    }
}
