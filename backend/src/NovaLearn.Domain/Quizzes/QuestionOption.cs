using NovaLearn.Domain.Common;

namespace NovaLearn.Domain.Quizzes;

/// <summary>
/// One selectable answer on a <see cref="Question"/>.
///
/// <see cref="IsCorrect"/> is the secret of the whole feature: it must never reach a learner
/// who is taking the quiz. Projections for taking a quiz deliberately omit it; see the
/// authoring and taking DTOs in the Application layer.
/// </summary>
public sealed class QuestionOption : BaseEntity
{
    private QuestionOption() { } // EF Core

    public Guid QuestionId { get; private set; }

    public string Text { get; private set; } = null!;

    public bool IsCorrect { get; private set; }

    /// <summary>Position within the question. Zero-based and never negative.</summary>
    public int SortOrder { get; private set; }

    public Question? Question { get; private set; }

    internal static QuestionOption Create(Guid questionId, string text, bool isCorrect, int sortOrder) =>
        new()
        {
            QuestionId = questionId,
            Text = text.Trim(),
            IsCorrect = isCorrect,
            SortOrder = sortOrder < 0 ? 0 : sortOrder
        };

    internal void Update(string text, bool isCorrect, int sortOrder)
    {
        Text = text.Trim();
        IsCorrect = isCorrect;
        SortOrder = sortOrder < 0 ? 0 : sortOrder;
    }
}
