using NovaLearn.Domain.Common;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Domain.Quizzes;

/// <summary>
/// One sitting of a quiz by one learner, and the aggregate root for its answers. Marking happens
/// inside <see cref="Submit"/>, so a score can never be set from outside.
/// </summary>
public sealed class QuizAttempt : BaseEntity
{
    private readonly List<AttemptAnswer> _answers = [];

    private QuizAttempt() { } // EF Core

    public Guid QuizId { get; private set; }

    public Guid StudentId { get; private set; }

    /// <summary>One-based sitting number, so a result can say "attempt 2 of 3".</summary>
    public int AttemptNumber { get; private set; }

    public DateTimeOffset StartedAtUtc { get; private set; }

    public DateTimeOffset? SubmittedAtUtc { get; private set; }

    public AttemptStatus Status { get; private set; }

    public int PointsAwarded { get; private set; }

    /// <summary>The quiz total captured at submission, so later edits cannot rewrite a result.</summary>
    public int TotalPoints { get; private set; }

    public double ScorePercent { get; private set; }

    public bool IsPassed { get; private set; }

    /// <summary>Whether the attempt was handed in after its time limit expired.</summary>
    public bool WasLate { get; private set; }

    public Quiz? Quiz { get; private set; }

    public ApplicationUser? Student { get; private set; }

    public IReadOnlyCollection<AttemptAnswer> Answers => _answers.AsReadOnly();

    public static QuizAttempt Start(Guid quizId, Guid studentId, int attemptNumber, DateTimeOffset startedAtUtc) =>
        new()
        {
            QuizId = quizId,
            StudentId = studentId,
            AttemptNumber = attemptNumber < 1 ? 1 : attemptNumber,
            StartedAtUtc = startedAtUtc,
            Status = AttemptStatus.InProgress
        };

    /// <summary>
    /// Records or replaces the response to one question. Ignored once submitted, so a marked
    /// attempt cannot be edited after the fact.
    /// </summary>
    public AttemptAnswer? Respond(Guid questionId, Guid? selectedOptionId, string? textAnswer)
    {
        if (Status == AttemptStatus.Submitted)
        {
            return null;
        }

        AttemptAnswer? existing = _answers.FirstOrDefault(a => a.QuestionId == questionId);
        if (existing is not null)
        {
            existing.Respond(selectedOptionId, textAnswer);
            return existing;
        }

        AttemptAnswer answer = AttemptAnswer.Create(Id, questionId, selectedOptionId, textAnswer);
        _answers.Add(answer);
        return answer;
    }

    /// <summary>
    /// Marks and closes the attempt. Each question scores itself, so the rule for what counts as
    /// correct lives with the question rather than here. Unanswered questions simply score zero.
    /// </summary>
    public void Submit(Quiz quiz, DateTimeOffset submittedAtUtc)
    {
        if (Status == AttemptStatus.Submitted)
        {
            return;
        }

        int awarded = 0;

        foreach (Question question in quiz.Questions)
        {
            AttemptAnswer? answer = _answers.FirstOrDefault(a => a.QuestionId == question.Id);
            if (answer is null)
            {
                continue;
            }

            int points = question.Mark(answer.SelectedOptionId, answer.TextAnswer);
            answer.Mark(points, points > 0);
            awarded += points;
        }

        PointsAwarded = awarded;
        TotalPoints = quiz.TotalPoints;
        ScorePercent = TotalPoints == 0 ? 0 : Math.Round(awarded * 100.0 / TotalPoints, 1);
        IsPassed = quiz.IsPass(ScorePercent);

        WasLate = quiz.DeadlineFor(StartedAtUtc) is { } deadline && submittedAtUtc > deadline;

        SubmittedAtUtc = submittedAtUtc;
        Status = AttemptStatus.Submitted;
    }
}
