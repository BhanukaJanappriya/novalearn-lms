using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Domain.Quizzes;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.DeleteQuiz;

public sealed class DeleteQuizCommandHandler(
    IQuizRepository quizzes,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteQuizCommand, Result>
{
    public async Task<Result> Handle(DeleteQuizCommand request, CancellationToken cancellationToken)
    {
        Quiz? quiz = await quizzes.GetQuizWithQuestionsAsync(request.QuizId, cancellationToken);
        if (quiz is null)
        {
            return Result.Failure(QuizErrors.QuizNotFound);
        }

        if (QuizAuthority.CheckCanManage(quiz.Course, currentUser) is { } denied)
        {
            return Result.Failure(denied);
        }

        // Soft delete, cascading to questions and options. Attempts keep their rows, so a
        // learner's recorded result survives the quiz being withdrawn.
        quizzes.RemoveQuiz(quiz);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
