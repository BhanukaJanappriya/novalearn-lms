using NovaLearn.Domain.Common;

namespace NovaLearn.Domain.Quizzes;

/// <summary>
/// A learner's answer to one question within an attempt. Carries the marking outcome once the
/// attempt is submitted, so a result can be shown without re-running the scorer.
/// </summary>
public sealed class AttemptAnswer : BaseEntity
{
    private AttemptAnswer() { } // EF Core

    public Guid AttemptId { get; private set; }

    public Guid QuestionId { get; private set; }

    /// <summary>The chosen option, for the option-based question types.</summary>
    public Guid? SelectedOptionId { get; private set; }

    /// <summary>The typed answer, for a short-answer question.</summary>
    public string? TextAnswer { get; private set; }

    /// <summary>Null until the attempt is submitted and marked.</summary>
    public bool? IsCorrect { get; private set; }

    public int PointsAwarded { get; private set; }

    public QuizAttempt? Attempt { get; private set; }

    public Question? Question { get; private set; }

    internal static AttemptAnswer Create(
        Guid attemptId, Guid questionId, Guid? selectedOptionId, string? textAnswer) =>
        new()
        {
            AttemptId = attemptId,
            QuestionId = questionId,
            SelectedOptionId = selectedOptionId,
            TextAnswer = Normalise(textAnswer)
        };

    /// <summary>Replaces the response. Only meaningful while the attempt is still open.</summary>
    internal void Respond(Guid? selectedOptionId, string? textAnswer)
    {
        SelectedOptionId = selectedOptionId;
        TextAnswer = Normalise(textAnswer);
    }

    /// <summary>Records the marking outcome for this answer.</summary>
    internal void Mark(int pointsAwarded, bool isCorrect)
    {
        PointsAwarded = pointsAwarded;
        IsCorrect = isCorrect;
    }

    private static string? Normalise(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
