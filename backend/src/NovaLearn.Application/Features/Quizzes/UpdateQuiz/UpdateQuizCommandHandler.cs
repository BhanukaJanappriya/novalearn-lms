using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Quizzes;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.UpdateQuiz;

public sealed class UpdateQuizCommandHandler(
    IQuizRepository quizzes,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<UpdateQuizCommand, Result<QuizSummaryDto>>
{
    public async Task<Result<QuizSummaryDto>> Handle(
        UpdateQuizCommand request, CancellationToken cancellationToken)
    {
        Quiz? quiz = await quizzes.GetQuizWithQuestionsAsync(request.QuizId, cancellationToken);
        if (quiz is null)
        {
            return Result.Failure<QuizSummaryDto>(QuizErrors.QuizNotFound);
        }

        if (QuizAuthority.CheckCanManage(quiz.Course, currentUser) is { } denied)
        {
            return Result.Failure<QuizSummaryDto>(denied);
        }

        // Publishing a quiz with no questions, or with a question that has no correct answer,
        // would hand learners something unanswerable that still scores against them.
        if (request.Status == AssessmentStatus.Published && !quiz.IsReadyToPublish())
        {
            return Result.Failure<QuizSummaryDto>(QuizErrors.NotReadyToPublish);
        }

        quiz.Update(
            request.Title,
            request.Description,
            request.TimeLimitMinutes,
            request.MaxAttempts,
            request.PassingScorePercent,
            request.ShuffleQuestions,
            request.Status);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return QuizSummaryDto.ForStaff(quiz);
    }
}
