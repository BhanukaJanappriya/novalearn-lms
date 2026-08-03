using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Quizzes;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.CreateQuiz;

public sealed class CreateQuizCommandHandler(
    ICourseRepository courses,
    IQuizRepository quizzes,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<CreateQuizCommand, Result<QuizSummaryDto>>
{
    public async Task<Result<QuizSummaryDto>> Handle(
        CreateQuizCommand request, CancellationToken cancellationToken)
    {
        Course? course = await courses.GetByIdAsync(request.CourseId, cancellationToken);
        if (course is null)
        {
            return Result.Failure<QuizSummaryDto>(QuizErrors.CourseNotFound);
        }

        if (QuizAuthority.CheckCanManage(course, currentUser) is { } denied)
        {
            return Result.Failure<QuizSummaryDto>(denied);
        }

        // A new quiz has no questions, so it cannot be published yet whatever was asked for.
        AssessmentStatus status = request.Status == AssessmentStatus.Published
            ? AssessmentStatus.Draft
            : request.Status;

        Quiz quiz = Quiz.Create(
            request.CourseId,
            request.Title,
            request.Description,
            request.TimeLimitMinutes,
            request.MaxAttempts,
            request.PassingScorePercent,
            request.ShuffleQuestions,
            status);

        await quizzes.AddQuizAsync(quiz, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return QuizSummaryDto.ForStaff(quiz);
    }
}
