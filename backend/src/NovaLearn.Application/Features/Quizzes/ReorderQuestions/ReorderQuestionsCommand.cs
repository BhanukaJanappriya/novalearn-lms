using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Domain.Quizzes;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.ReorderQuestions;

/// <summary>Rewrites question order from a full list of ids, in the order they should appear.</summary>
public sealed record ReorderQuestionsCommand(Guid QuizId, IReadOnlyList<Guid> QuestionIds)
    : IRequest<Result<QuizAuthoringDto>>;

public sealed class ReorderQuestionsCommandHandler(
    IQuizRepository quizzes,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<ReorderQuestionsCommand, Result<QuizAuthoringDto>>
{
    public async Task<Result<QuizAuthoringDto>> Handle(
        ReorderQuestionsCommand request, CancellationToken cancellationToken)
    {
        Quiz? quiz = await quizzes.GetQuizWithQuestionsAsync(request.QuizId, cancellationToken);
        if (quiz is null)
        {
            return Result.Failure<QuizAuthoringDto>(QuizErrors.QuizNotFound);
        }

        if (QuizAuthority.CheckCanManage(quiz.Course, currentUser) is { } denied)
        {
            return Result.Failure<QuizAuthoringDto>(denied);
        }

        // Every id must belong to this quiz, and the list must cover all of them. A partial list
        // would leave the unnamed questions with stale positions that collide with the new ones.
        HashSet<Guid> existing = quiz.Questions.Select(q => q.Id).ToHashSet();
        if (!existing.SetEquals(request.QuestionIds))
        {
            return Result.Failure<QuizAuthoringDto>(QuizErrors.QuestionNotFound);
        }

        for (int position = 0; position < request.QuestionIds.Count; position++)
        {
            quiz.Questions.First(q => q.Id == request.QuestionIds[position]).MoveTo(position);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return QuizAuthoringDto.FromEntity(quiz);
    }
}
