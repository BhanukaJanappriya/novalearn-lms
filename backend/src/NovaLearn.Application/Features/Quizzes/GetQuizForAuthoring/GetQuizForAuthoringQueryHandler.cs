using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Domain.Quizzes;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.GetQuizForAuthoring;

public sealed class GetQuizForAuthoringQueryHandler(
    IQuizRepository quizzes,
    ICurrentUser currentUser)
    : IRequestHandler<GetQuizForAuthoringQuery, Result<QuizAuthoringDto>>
{
    public async Task<Result<QuizAuthoringDto>> Handle(
        GetQuizForAuthoringQuery request, CancellationToken cancellationToken)
    {
        Quiz? quiz = await quizzes.GetQuizWithQuestionsAsync(request.QuizId, cancellationToken);
        if (quiz is null)
        {
            return Result.Failure<QuizAuthoringDto>(QuizErrors.QuizNotFound);
        }

        // The only gate on the answer key.
        if (QuizAuthority.CheckCanManage(quiz.Course, currentUser) is { } denied)
        {
            return Result.Failure<QuizAuthoringDto>(denied);
        }

        return QuizAuthoringDto.FromEntity(quiz);
    }
}
