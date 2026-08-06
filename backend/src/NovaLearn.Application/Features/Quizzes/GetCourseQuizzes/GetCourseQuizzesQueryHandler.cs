using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Quizzes;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.GetCourseQuizzes;

public sealed class GetCourseQuizzesQueryHandler(
    ICourseRepository courses,
    IQuizRepository quizzes,
    IEnrollmentRepository enrollments,
    ICurrentUser currentUser)
    : IRequestHandler<GetCourseQuizzesQuery, Result<IReadOnlyList<QuizSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<QuizSummaryDto>>> Handle(
        GetCourseQuizzesQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId)
        {
            return Result.Failure<IReadOnlyList<QuizSummaryDto>>(QuizErrors.Unauthenticated);
        }

        Course? course = await courses.GetByIdAsync(request.CourseId, cancellationToken);
        if (course is null)
        {
            return Result.Failure<IReadOnlyList<QuizSummaryDto>>(QuizErrors.CourseNotFound);
        }

        bool isStaff = QuizAuthority.CheckCanManage(course, currentUser) is null;

        IReadOnlyList<Quiz> all = await quizzes.ListQuizzesAsync(request.CourseId, cancellationToken);

        if (isStaff)
        {
            return all.Select(QuizSummaryDto.ForStaff).ToList();
        }

        Enrollment? enrollment =
            await enrollments.GetActiveAsync(callerId, request.CourseId, cancellationToken);

        if (enrollment is null)
        {
            return Result.Failure<IReadOnlyList<QuizSummaryDto>>(QuizErrors.NotEnrolled);
        }

        // One query for every attempt the learner has made on this course, rather than one per quiz.
        IReadOnlyList<QuizAttempt> mine =
            await quizzes.ListAttemptsForCourseAsync(request.CourseId, callerId, cancellationToken);

        ILookup<Guid, QuizAttempt> byQuiz = mine
            .Where(a => a.Status != AttemptStatus.InProgress)
            .ToLookup(a => a.QuizId);

        return all
            .Where(q => q.Status == AssessmentStatus.Published)
            .Select(q => QuizSummaryDto.ForLearner(q, byQuiz[q.Id].ToList()))
            .ToList();
    }
}
