using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Quizzes;

namespace NovaLearn.Persistence.Repositories;

public sealed class QuizRepository(ApplicationDbContext dbContext) : IQuizRepository
{
    public async Task AddQuizAsync(Quiz quiz, CancellationToken cancellationToken) =>
        await dbContext.Quizzes.AddAsync(quiz, cancellationToken);

    public Task<Quiz?> GetQuizAsync(Guid quizId, CancellationToken cancellationToken) =>
        dbContext.Quizzes
            .Include(q => q.Course)
            .FirstOrDefaultAsync(q => q.Id == quizId, cancellationToken);

    public Task<Quiz?> GetQuizWithQuestionsAsync(Guid quizId, CancellationToken cancellationToken) =>
        dbContext.Quizzes
            .Include(q => q.Course)
            .Include(q => q.Questions.OrderBy(question => question.SortOrder))
            .ThenInclude(question => question.Options.OrderBy(o => o.SortOrder))
            .FirstOrDefaultAsync(q => q.Id == quizId, cancellationToken);

    public async Task<IReadOnlyList<Quiz>> ListQuizzesAsync(
        Guid courseId, CancellationToken cancellationToken) =>
        await dbContext.Quizzes
            .Include(q => q.Questions)
            .Where(q => q.CourseId == courseId)
            .OrderByDescending(q => q.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public void RemoveQuiz(Quiz quiz)
    {
        // The database cascade only fires on a hard delete; because the interceptor turns these
        // into soft deletes, the children have to be marked explicitly.
        foreach (Question question in quiz.Questions.ToList())
        {
            RemoveQuestion(question);
        }

        dbContext.Quizzes.Remove(quiz);
    }

    public async Task AddQuestionAsync(Question question, CancellationToken cancellationToken) =>
        await dbContext.QuizQuestions.AddAsync(question, cancellationToken);

    public Task<Question?> GetQuestionAsync(Guid questionId, CancellationToken cancellationToken) =>
        dbContext.QuizQuestions
            .Include(q => q.Options.OrderBy(o => o.SortOrder))
            .Include(q => q.Quiz)
            .ThenInclude(quiz => quiz!.Course)
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

    public void RemoveQuestion(Question question)
    {
        foreach (QuestionOption option in question.Options.ToList())
        {
            dbContext.Set<QuestionOption>().Remove(option);
        }

        dbContext.QuizQuestions.Remove(question);
    }

    public async Task ReplaceOptionsAsync(
        Question question, IEnumerable<QuestionOption> options, CancellationToken cancellationToken)
    {
        // The domain has already cleared the collection, so the previously loaded rows are only
        // reachable from the change tracker. Delete them explicitly, then state the inserts.
        List<QuestionOption> existing = await dbContext.Set<QuestionOption>()
            .Where(o => o.QuestionId == question.Id)
            .ToListAsync(cancellationToken);

        foreach (QuestionOption option in existing)
        {
            dbContext.Set<QuestionOption>().Remove(option);
        }

        await dbContext.Set<QuestionOption>().AddRangeAsync(options, cancellationToken);
    }

    public async Task AddAttemptAsync(QuizAttempt attempt, CancellationToken cancellationToken) =>
        await dbContext.QuizAttempts.AddAsync(attempt, cancellationToken);

    public Task<QuizAttempt?> GetAttemptAsync(Guid attemptId, CancellationToken cancellationToken) =>
        dbContext.QuizAttempts
            .Include(a => a.Answers)
            .Include(a => a.Student)
            .Include(a => a.Quiz)
            .ThenInclude(q => q!.Course)
            .FirstOrDefaultAsync(a => a.Id == attemptId, cancellationToken);

    public async Task AddAnswerAsync(AttemptAnswer answer, CancellationToken cancellationToken) =>
        await dbContext.Set<AttemptAnswer>().AddAsync(answer, cancellationToken);

    public async Task<IReadOnlyList<QuizAttempt>> ListAttemptsForStudentAsync(
        Guid quizId, Guid studentId, CancellationToken cancellationToken) =>
        await dbContext.QuizAttempts
            .Where(a => a.QuizId == quizId && a.StudentId == studentId)
            .OrderByDescending(a => a.StartedAtUtc)
            .ToListAsync(cancellationToken);

    public Task<QuizAttempt?> GetOpenAttemptAsync(
        Guid quizId, Guid studentId, CancellationToken cancellationToken) =>
        dbContext.QuizAttempts
            .Include(a => a.Answers)
            .FirstOrDefaultAsync(
                a => a.QuizId == quizId
                    && a.StudentId == studentId
                    && a.Status == AttemptStatus.InProgress,
                cancellationToken);

    public async Task<IReadOnlyList<QuizAttempt>> ListAttemptsAsync(
        Guid quizId, CancellationToken cancellationToken) =>
        await dbContext.QuizAttempts
            .Include(a => a.Student)
            .Where(a => a.QuizId == quizId && a.Status != AttemptStatus.InProgress)
            .OrderByDescending(a => a.SubmittedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<QuizAttempt>> ListAttemptsForCourseAsync(
        Guid courseId, Guid studentId, CancellationToken cancellationToken) =>
        await dbContext.QuizAttempts
            .Where(a => a.StudentId == studentId && a.Quiz!.CourseId == courseId)
            .ToListAsync(cancellationToken);
}
