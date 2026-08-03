using NovaLearn.Domain.Quizzes;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>
/// Persistence port for the <see cref="Quiz"/> aggregate and its attempts. Attempts live here
/// rather than on their own port because every use case that touches one also needs its quiz,
/// for the questions that do the marking.
/// </summary>
public interface IQuizRepository
{
    Task AddQuizAsync(Quiz quiz, CancellationToken cancellationToken);

    /// <summary>A quiz with its course, but without questions. Enough for authority checks.</summary>
    Task<Quiz?> GetQuizAsync(Guid quizId, CancellationToken cancellationToken);

    /// <summary>
    /// A quiz with its course, questions and every question's options. This is the shape marking
    /// needs, since a question can only score an answer when its options are loaded.
    /// </summary>
    Task<Quiz?> GetQuizWithQuestionsAsync(Guid quizId, CancellationToken cancellationToken);

    /// <summary>A course's quizzes, newest first, with questions loaded for the counts.</summary>
    Task<IReadOnlyList<Quiz>> ListQuizzesAsync(Guid courseId, CancellationToken cancellationToken);

    void RemoveQuiz(Quiz quiz);

    /// <summary>
    /// Tracks a question as an insert. <see cref="Domain.Common.BaseEntity"/> assigns the key
    /// client-side, so an entity reached only through a navigation is tracked as Modified and
    /// saves as a no-op UPDATE. The insert has to be stated explicitly.
    /// </summary>
    Task AddQuestionAsync(Question question, CancellationToken cancellationToken);

    /// <summary>A question with its options and its quiz, for editing and ownership checks.</summary>
    Task<Question?> GetQuestionAsync(Guid questionId, CancellationToken cancellationToken);

    void RemoveQuestion(Question question);

    /// <summary>Replaces a question's options, deleting the old rows and inserting the new ones.</summary>
    Task ReplaceOptionsAsync(
        Question question, IEnumerable<QuestionOption> options, CancellationToken cancellationToken);

    Task AddAttemptAsync(QuizAttempt attempt, CancellationToken cancellationToken);

    /// <summary>An attempt with its answers, quiz and learner.</summary>
    Task<QuizAttempt?> GetAttemptAsync(Guid attemptId, CancellationToken cancellationToken);

    /// <summary>Tracks an answer as an insert, for the same reason as <see cref="AddQuestionAsync"/>.</summary>
    Task AddAnswerAsync(AttemptAnswer answer, CancellationToken cancellationToken);

    /// <summary>A learner's attempts at one quiz, newest first.</summary>
    Task<IReadOnlyList<QuizAttempt>> ListAttemptsForStudentAsync(
        Guid quizId, Guid studentId, CancellationToken cancellationToken);

    /// <summary>A learner's still-open attempt at a quiz, if there is one.</summary>
    Task<QuizAttempt?> GetOpenAttemptAsync(Guid quizId, Guid studentId, CancellationToken cancellationToken);

    /// <summary>Every submitted attempt at a quiz, with learners, for the results roster.</summary>
    Task<IReadOnlyList<QuizAttempt>> ListAttemptsAsync(Guid quizId, CancellationToken cancellationToken);

    /// <summary>A learner's attempts across every quiz on one course, for their quiz list.</summary>
    Task<IReadOnlyList<QuizAttempt>> ListAttemptsForCourseAsync(
        Guid courseId, Guid studentId, CancellationToken cancellationToken);
}
