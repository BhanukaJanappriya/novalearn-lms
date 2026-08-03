using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Domain.Quizzes;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.DeleteQuestion;

public sealed class DeleteQuestionCommandHandler(
    IQuizRepository quizzes,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteQuestionCommand, Result>
{
    public async Task<Result> Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
    {
        Question? question = await quizzes.GetQuestionAsync(request.QuestionId, cancellationToken);
        if (question is null)
        {
            return Result.Failure(QuizErrors.QuestionNotFound);
        }

        if (QuizAuthority.CheckCanManage(question.Quiz?.Course, currentUser) is { } denied)
        {
            return Result.Failure(denied);
        }

        quizzes.RemoveQuestion(question);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
