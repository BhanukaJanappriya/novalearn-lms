using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Quizzes;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.SaveAnswer;

public sealed class SaveAnswerCommandHandler(
    IQuizRepository quizzes,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<SaveAnswerCommand, Result>
{
    public async Task<Result> Handle(SaveAnswerCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } studentId)
        {
            return Result.Failure(QuizErrors.Unauthenticated);
        }

        QuizAttempt? attempt = await quizzes.GetAttemptAsync(request.AttemptId, cancellationToken);
        if (attempt is null)
        {
            return Result.Failure(QuizErrors.AttemptNotFound);
        }

        // Nobody else's attempt, not even an administrator's: answering for someone would
        // fabricate their result.
        if (attempt.StudentId != studentId)
        {
            return Result.Failure(QuizErrors.NotAttemptOwner);
        }

        if (attempt.Status == AttemptStatus.Submitted)
        {
            return Result.Failure(QuizErrors.AttemptAlreadySubmitted);
        }

        Quiz? quiz = await quizzes.GetQuizWithQuestionsAsync(attempt.QuizId, cancellationToken);
        if (quiz is null)
        {
            return Result.Failure(QuizErrors.QuizNotFound);
        }

        // Stops an answer being planted against a question from another quiz.
        if (quiz.Questions.All(q => q.Id != request.QuestionId))
        {
            return Result.Failure(QuizErrors.QuestionNotInAttempt);
        }

        bool isNew = attempt.Answers.All(a => a.QuestionId != request.QuestionId);

        AttemptAnswer? answer = attempt.Respond(
            request.QuestionId, request.SelectedOptionId, request.TextAnswer);

        if (answer is null)
        {
            return Result.Failure(QuizErrors.AttemptAlreadySubmitted);
        }

        if (isNew)
        {
            // Stated explicitly: BaseEntity assigns the key client-side, so a new answer reached
            // only through the attempt's collection would save as a no-op UPDATE.
            await quizzes.AddAnswerAsync(answer, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
