using NovaLearn.Domain.Common;

namespace NovaLearn.Domain.Quizzes;

/// <summary>
/// One question and the aggregate root for its <see cref="QuestionOption"/> children.
/// Owns its own marking rule, so scoring an attempt is the question answering "is this right?"
/// rather than a scorer switching on the type from outside.
/// </summary>
public sealed class Question : BaseEntity
{
    /// <summary>Upper bound on <see cref="Points"/>, keeping quiz totals on a sane scale.</summary>
    public const int MaxPointsCeiling = 100;

    /// <summary>Separator for the accepted-answer list, stored as one column.</summary>
    private const char AcceptedAnswerSeparator = '\n';

    private readonly List<QuestionOption> _options = [];

    private Question() { } // EF Core

    public Guid QuizId { get; private set; }

    public string Text { get; private set; } = null!;

    public QuestionType Type { get; private set; }

    /// <summary>What a correct answer is worth. Always at least 1.</summary>
    public int Points { get; private set; }

    /// <summary>Position within the quiz. Zero-based and never negative.</summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Newline-separated answers accepted for a <see cref="QuestionType.ShortAnswer"/>.
    /// Null for the option-based types.
    /// </summary>
    public string? AcceptedAnswers { get; private set; }

    public Quiz? Quiz { get; private set; }

    /// <summary>The question's options. Mutate through <see cref="ReplaceOptions"/>.</summary>
    public IReadOnlyCollection<QuestionOption> Options => _options.AsReadOnly();

    /// <summary>The accepted answers as a list, trimmed and empties removed.</summary>
    public IReadOnlyList<string> AcceptedAnswerList =>
        string.IsNullOrWhiteSpace(AcceptedAnswers)
            ? []
            : AcceptedAnswers
                .Split(AcceptedAnswerSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

    public static Question Create(
        Guid quizId,
        string text,
        QuestionType type,
        int points,
        int sortOrder,
        IEnumerable<string>? acceptedAnswers = null)
    {
        var question = new Question
        {
            QuizId = quizId,
            Text = text.Trim(),
            Type = type,
            Points = ClampPoints(points),
            SortOrder = sortOrder < 0 ? 0 : sortOrder
        };

        question.SetAcceptedAnswers(acceptedAnswers);
        return question;
    }

    /// <summary>Applies edited details, keeping the same invariants as <see cref="Create"/>.</summary>
    public void Update(string text, QuestionType type, int points, IEnumerable<string>? acceptedAnswers)
    {
        Text = text.Trim();
        Type = type;
        Points = ClampPoints(points);
        SetAcceptedAnswers(acceptedAnswers);

        // Changing to a text question makes any options meaningless.
        if (type == QuestionType.ShortAnswer)
        {
            _options.Clear();
        }
    }

    public void MoveTo(int sortOrder) => SortOrder = sortOrder < 0 ? 0 : sortOrder;

    /// <summary>
    /// Replaces the option set wholesale. Editing options individually would let a question sit
    /// in a half-valid state (no correct answer, or several), which scoring cannot resolve.
    /// </summary>
    public IReadOnlyList<QuestionOption> ReplaceOptions(IEnumerable<(string Text, bool IsCorrect)> options)
    {
        _options.Clear();

        if (Type == QuestionType.ShortAnswer)
        {
            return [];
        }

        int order = 0;
        foreach ((string text, bool isCorrect) in options)
        {
            _options.Add(QuestionOption.Create(Id, text, isCorrect, order++));
        }

        return _options.AsReadOnly();
    }

    /// <summary>
    /// Whether this question is complete enough to be answered. An option question needs at
    /// least two options and exactly one correct; a text question needs an accepted answer.
    /// </summary>
    public bool IsAnswerable() => Type switch
    {
        QuestionType.ShortAnswer => AcceptedAnswerList.Count > 0,
        _ => _options.Count >= 2 && _options.Count(o => o.IsCorrect) == 1
    };

    /// <summary>
    /// Marks an answer, returning the points earned. All or nothing: there is no partial credit
    /// on a single question.
    /// </summary>
    public int Mark(Guid? selectedOptionId, string? textAnswer)
    {
        bool correct = Type switch
        {
            QuestionType.ShortAnswer => MatchesAcceptedAnswer(textAnswer),
            _ => selectedOptionId is { } id && _options.Any(o => o.Id == id && o.IsCorrect)
        };

        return correct ? Points : 0;
    }

    /// <summary>Case-insensitive, whitespace-tolerant comparison against the accepted answers.</summary>
    private bool MatchesAcceptedAnswer(string? textAnswer)
    {
        if (string.IsNullOrWhiteSpace(textAnswer))
        {
            return false;
        }

        string normalised = textAnswer.Trim();

        return AcceptedAnswerList.Any(accepted =>
            string.Equals(accepted, normalised, StringComparison.OrdinalIgnoreCase));
    }

    private void SetAcceptedAnswers(IEnumerable<string>? acceptedAnswers)
    {
        if (acceptedAnswers is null)
        {
            AcceptedAnswers = null;
            return;
        }

        List<string> cleaned = acceptedAnswers
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a.Trim())
            .ToList();

        AcceptedAnswers = cleaned.Count == 0 ? null : string.Join(AcceptedAnswerSeparator, cleaned);
    }

    private static int ClampPoints(int points) => Math.Clamp(points, 1, MaxPointsCeiling);
}
