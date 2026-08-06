using NovaLearn.Domain.Common;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Domain.Quizzes;

/// <summary>
/// A learner's answer to one question within an attempt. Carries the marking outcome once the
/// attempt is submitted, so a result can be shown without re-running the scorer.
/// </summary>
public sealed class AttemptAnswer : BaseEntity
{
    /// <summary>Separator for the selected-option list, stored as one column.</summary>
    private const char OptionSeparator = ',';

    private AttemptAnswer() { } // EF Core

    public Guid AttemptId { get; private set; }

    public Guid QuestionId { get; private set; }

    /// <summary>
    /// Comma-separated ids of the chosen options. A single-choice question stores one, a
    /// multiple-response question stores several, and a text question stores none.
    /// </summary>
    public string? SelectedOptionIds { get; private set; }

    /// <summary>The typed answer, for a short-answer or essay question.</summary>
    public string? TextAnswer { get; private set; }

    /// <summary>Null until the answer is marked. An unmarked essay keeps this null.</summary>
    public bool? IsCorrect { get; private set; }

    public int PointsAwarded { get; private set; }

    /// <summary>
    /// Captured from the question when the attempt is handed in, so a later edit to the question
    /// type cannot change whether an already-submitted answer was waiting on a person.
    /// </summary>
    public bool RequiresManualMarking { get; private set; }

    /// <summary>Whether a person has marked this answer. Always false until they do.</summary>
    public bool IsManuallyMarked { get; private set; }

    /// <summary>The marker's comment on an essay answer.</summary>
    public string? Feedback { get; private set; }

    public Guid? MarkedById { get; private set; }

    public DateTimeOffset? MarkedAtUtc { get; private set; }

    public QuizAttempt? Attempt { get; private set; }

    public Question? Question { get; private set; }

    /// <summary>The chosen option ids as a list.</summary>
    public IReadOnlyList<Guid> SelectedOptions =>
        string.IsNullOrWhiteSpace(SelectedOptionIds)
            ? []
            : SelectedOptionIds
                .Split(OptionSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(id => Guid.TryParse(id, out Guid parsed) ? parsed : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .ToList();

    /// <summary>Whether this answer is still waiting for a person.</summary>
    public bool IsAwaitingMarking => RequiresManualMarking && !IsManuallyMarked;

    /// <summary>Whether the learner actually put something here.</summary>
    public bool HasResponse =>
        SelectedOptions.Count > 0 || !string.IsNullOrWhiteSpace(TextAnswer);

    internal static AttemptAnswer Create(
        Guid attemptId, Guid questionId, IReadOnlyCollection<Guid> selectedOptionIds, string? textAnswer) =>
        new()
        {
            AttemptId = attemptId,
            QuestionId = questionId,
            SelectedOptionIds = Join(selectedOptionIds),
            TextAnswer = Normalise(textAnswer)
        };

    /// <summary>Replaces the response. Only meaningful while the attempt is still open.</summary>
    internal void Respond(IReadOnlyCollection<Guid> selectedOptionIds, string? textAnswer)
    {
        SelectedOptionIds = Join(selectedOptionIds);
        TextAnswer = Normalise(textAnswer);
    }

    /// <summary>Records the automatic marking outcome for this answer.</summary>
    internal void MarkAutomatically(int pointsAwarded, bool isCorrect)
    {
        PointsAwarded = pointsAwarded;
        IsCorrect = isCorrect;
        RequiresManualMarking = false;
    }

    /// <summary>Flags this answer as one a person has to look at, and leaves it unscored.</summary>
    internal void DeferToMarker()
    {
        PointsAwarded = 0;
        IsCorrect = null;
        RequiresManualMarking = true;
        IsManuallyMarked = false;
    }

    /// <summary>
    /// Records a person's mark. Points are clamped to what the question is worth, so a marker
    /// cannot award more than the essay carries.
    /// </summary>
    internal void MarkManually(
        int points, string? feedback, int maxPoints, Guid markedById, DateTimeOffset markedAtUtc)
    {
        PointsAwarded = Math.Clamp(points, 0, maxPoints);
        Feedback = Normalise(feedback);
        MarkedById = markedById;
        MarkedAtUtc = markedAtUtc;
        IsManuallyMarked = true;

        // An essay is rarely all or nothing, so "correct" means it earned full marks.
        IsCorrect = PointsAwarded == maxPoints;
    }

    private static string? Join(IReadOnlyCollection<Guid> ids) =>
        ids.Count == 0 ? null : string.Join(OptionSeparator, ids);

    private static string? Normalise(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
