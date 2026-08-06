using NovaLearn.Domain.Common;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Domain.Quizzes;

/// <summary>
/// One sitting of a quiz by one learner, and the aggregate root for its answers. Marking happens
/// inside <see cref="Submit"/> and <see cref="MarkEssay"/>, so a score can never be set from
/// outside.
///
/// An attempt containing essays is auto-marked on submission and then waits: the score it shows
/// is provisional until a person has marked the written answers.
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

    /// <summary>When the last essay was marked and the score became final.</summary>
    public DateTimeOffset? MarkedAtUtc { get; private set; }

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

    /// <summary>How many written answers are still waiting for a person.</summary>
    public int AwaitingMarkingCount => _answers.Count(a => a.IsAwaitingMarking);

    /// <summary>Whether the score on show is provisional.</summary>
    public bool HasPendingManualMarking => Status == AttemptStatus.PendingReview;

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
    public AttemptAnswer? Respond(
        Guid questionId, IReadOnlyCollection<Guid> selectedOptionIds, string? textAnswer)
    {
        if (Status != AttemptStatus.InProgress)
        {
            return null;
        }

        AttemptAnswer? existing = _answers.FirstOrDefault(a => a.QuestionId == questionId);
        if (existing is not null)
        {
            existing.Respond(selectedOptionIds, textAnswer);
            return existing;
        }

        AttemptAnswer answer = AttemptAnswer.Create(Id, questionId, selectedOptionIds, textAnswer);
        _answers.Add(answer);
        return answer;
    }

    /// <summary>Required questions the learner has left blank, so submission can be refused.</summary>
    public IReadOnlyList<Question> UnansweredRequired(Quiz quiz) =>
        quiz.Questions
            .Where(q => q.IsRequired)
            .Where(q => _answers.FirstOrDefault(a => a.QuestionId == q.Id) is not { HasResponse: true })
            .ToList();

    /// <summary>
    /// Closes the attempt and marks everything a machine can. Each question scores itself, so the
    /// rule for what counts as correct lives with the question rather than here.
    ///
    /// Essays the learner actually wrote are deferred to a person. An essay left blank is scored
    /// zero outright, so the marking queue only ever holds real work.
    /// </summary>
    public void Submit(Quiz quiz, DateTimeOffset submittedAtUtc)
    {
        if (Status != AttemptStatus.InProgress)
        {
            return;
        }

        foreach (Question question in quiz.Questions)
        {
            AttemptAnswer? answer = _answers.FirstOrDefault(a => a.QuestionId == question.Id);
            if (answer is null)
            {
                continue;
            }

            if (question.RequiresManualMarking && answer.HasResponse)
            {
                answer.DeferToMarker();
                continue;
            }

            int points = question.Mark(answer.SelectedOptions, answer.TextAnswer);
            answer.MarkAutomatically(points, points > 0);
        }

        TotalPoints = quiz.TotalPoints;
        WasLate = quiz.DeadlineFor(StartedAtUtc) is { } deadline && submittedAtUtc > deadline;
        SubmittedAtUtc = submittedAtUtc;

        Recalculate(quiz);
    }

    /// <summary>
    /// Records a person's mark on one essay answer, then finalises the attempt once nothing is
    /// left waiting.
    /// </summary>
    public bool MarkEssay(
        Guid answerId, int points, string? feedback, Quiz quiz, Guid markedById, DateTimeOffset markedAtUtc)
    {
        AttemptAnswer? answer = _answers.FirstOrDefault(a => a.Id == answerId);
        if (answer is null || !answer.RequiresManualMarking || Status == AttemptStatus.InProgress)
        {
            return false;
        }

        Question? question = quiz.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
        if (question is null)
        {
            return false;
        }

        answer.MarkManually(points, feedback, question.Points, markedById, markedAtUtc);

        Recalculate(quiz);

        if (Status == AttemptStatus.Graded)
        {
            MarkedAtUtc = markedAtUtc;
        }

        return true;
    }

    /// <summary>
    /// Recomputes the score from whatever is marked so far and settles the status. Called after
    /// submission and after every manual mark, so the two paths cannot drift apart.
    /// </summary>
    private void Recalculate(Quiz quiz)
    {
        PointsAwarded = _answers.Sum(a => a.PointsAwarded);
        ScorePercent = TotalPoints == 0 ? 0 : Math.Round(PointsAwarded * 100.0 / TotalPoints, 1);

        bool awaiting = _answers.Any(a => a.IsAwaitingMarking);

        Status = awaiting ? AttemptStatus.PendingReview : AttemptStatus.Graded;

        // A pass is a statement about a final score, so it stays false while marking is pending.
        IsPassed = !awaiting && quiz.IsPass(ScorePercent);
    }
}
