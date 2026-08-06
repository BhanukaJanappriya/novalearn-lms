using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Quizzes;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.StartAttempt;

public sealed class StartAttemptCommandHandler(
    IQuizRepository quizzes,
    IEnrollmentRepository enrollments,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider dateTime)
    : IRequestHandler<StartAttemptCommand, Result<AttemptInProgressDto>>
{
    public async Task<Result<AttemptInProgressDto>> Handle(
        StartAttemptCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } studentId)
        {
            return Result.Failure<AttemptInProgressDto>(QuizErrors.Unauthenticated);
        }

        Quiz? quiz = await quizzes.GetQuizWithQuestionsAsync(request.QuizId, cancellationToken);
        if (quiz is null)
        {
            return Result.Failure<AttemptInProgressDto>(QuizErrors.QuizNotFound);
        }

        if (quiz.Status != AssessmentStatus.Published)
        {
            return Result.Failure<AttemptInProgressDto>(QuizErrors.NotPublished);
        }

        // Enrolment is the gate, not role, matching how assignment submission works.
        Enrollment? enrollment =
            await enrollments.GetActiveAsync(studentId, quiz.CourseId, cancellationToken);

        if (enrollment is null)
        {
            return Result.Failure<AttemptInProgressDto>(QuizErrors.NotEnrolled);
        }

        // Resuming rather than starting afresh: a reload mid-quiz must not burn an attempt.
        QuizAttempt? open = await quizzes.GetOpenAttemptAsync(quiz.Id, studentId, cancellationToken);
        if (open is not null)
        {
            return Project(quiz, open);
        }

        IReadOnlyList<QuizAttempt> previous =
            await quizzes.ListAttemptsForStudentAsync(quiz.Id, studentId, cancellationToken);

        // A pending-review attempt still counts as used: it has been handed in.
        int used = previous.Count(a => a.Status != AttemptStatus.InProgress);
        if (!quiz.AllowsAnotherAttempt(used))
        {
            return Result.Failure<AttemptInProgressDto>(QuizErrors.NoAttemptsLeft);
        }

        QuizAttempt attempt = QuizAttempt.Start(quiz.Id, studentId, used + 1, dateTime.UtcNow);

        await quizzes.AddAttemptAsync(attempt, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Project(quiz, attempt);
    }

    private static AttemptInProgressDto Project(Quiz quiz, QuizAttempt attempt)
    {
        Dictionary<Guid, AttemptAnswer> answers = attempt.Answers.ToDictionary(a => a.QuestionId);

        IEnumerable<Question> questions = quiz.Questions.OrderBy(q => q.SortOrder);

        if (quiz.ShuffleQuestions)
        {
            // Seeded by the attempt so a reload keeps the same order; a fresh shuffle every
            // request would move questions under the learner as they work.
            var random = new Random(attempt.Id.GetHashCode());
            questions = questions.OrderBy(_ => random.Next());
        }

        return new AttemptInProgressDto(
            attempt.Id,
            quiz.Id,
            quiz.Title,
            attempt.AttemptNumber,
            attempt.StartedAtUtc,
            quiz.DeadlineFor(attempt.StartedAtUtc),
            quiz.TotalPoints,
            questions
                .Select(q => TakingQuestionDto.FromEntity(
                    q, answers.TryGetValue(q.Id, out AttemptAnswer? answer) ? answer : null))
                .ToList());
    }
}
