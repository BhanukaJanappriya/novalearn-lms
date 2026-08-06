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

    /// <summary>Whether the learner must answer before they can hand the attempt in.</summary>
    public bool IsRequired { get; private set; }

    /// <summary>
    /// Newline-separated answers accepted for a <see cref="QuestionType.ShortAnswer"/>.
    /// Null for every other type.
    /// </summary>
    public string? AcceptedAnswers { get; private set; }

    /// <summary>Guidance shown to whoever marks an essay. Never shown to the learner.</summary>
    public string? MarkingGuidance { get; private set; }

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

    /// <summary>Whether this question can only be marked by a person.</summary>
    public bool RequiresManualMarking => Type == QuestionType.Essay;

    /// <summary>Whether this type is answered by picking from a list.</summary>
    public bool IsOptionBased =>
        Type is QuestionType.MultipleChoice or QuestionType.TrueFalse or QuestionType.MultipleResponse;

    /// <summary>Whether the learner may select more than one option.</summary>
    public bool AllowsMultipleSelections => Type == QuestionType.MultipleResponse;

    public static Question Create(
        Guid quizId,
        string text,
        QuestionType type,
        int points,
        int sortOrder,
        IEnumerable<string>? acceptedAnswers = null,
        bool isRequired = false,
        string? markingGuidance = null)
    {
        var question = new Question
        {
            QuizId = quizId,
            Text = text.Trim(),
            Type = type,
            Points = ClampPoints(points),
            SortOrder = sortOrder < 0 ? 0 : sortOrder,
            IsRequired = isRequired
        };

        question.SetAcceptedAnswers(acceptedAnswers);
        question.SetMarkingGuidance(markingGuidance);
        return question;
    }

    /// <summary>Applies edited details, keeping the same invariants as <see cref="Create"/>.</summary>
    public void Update(
        string text,
        QuestionType type,
        int points,
        IEnumerable<string>? acceptedAnswers,
        bool isRequired = false,
        string? markingGuidance = null)
    {
        Text = text.Trim();
        Type = type;
        Points = ClampPoints(points);
        IsRequired = isRequired;

        SetAcceptedAnswers(acceptedAnswers);
        SetMarkingGuidance(markingGuidance);

        // Options only mean anything for the pick-from-a-list types.
        if (!IsOptionBased)
        {
            _options.Clear();
        }
    }

    public void MoveTo(int sortOrder) => SortOrder = sortOrder < 0 ? 0 : sortOrder;

    /// <summary>
    /// Replaces the option set wholesale. Editing options individually would let a question sit
    /// in a half-valid state (no correct answer, or too many), which scoring cannot resolve.
    /// </summary>
    public IReadOnlyList<QuestionOption> ReplaceOptions(IEnumerable<(string Text, bool IsCorrect)> options)
    {
        _options.Clear();

        if (!IsOptionBased)
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
    /// Whether this question is complete enough to be answered.
    ///
    /// An essay is always answerable: having no answer key is the point, since a person supplies
    /// the judgement instead.
    /// </summary>
    public bool IsAnswerable() => Type switch
    {
        QuestionType.Essay => true,
        QuestionType.ShortAnswer => AcceptedAnswerList.Count > 0,
        QuestionType.MultipleResponse => _options.Count >= 2 && _options.Any(o => o.IsCorrect),
        _ => _options.Count >= 2 && _options.Count(o => o.IsCorrect) == 1
    };

    /// <summary>
    /// Marks an answer, returning the points earned. All or nothing: a multiple-response question
    /// must match the correct set exactly, so half-right scores nothing.
    ///
    /// Returns zero for an essay, which the caller must not treat as a mark. Use
    /// <see cref="RequiresManualMarking"/> to tell "scored zero" apart from "not scored yet".
    /// </summary>
    public int Mark(IReadOnlyCollection<Guid> selectedOptionIds, string? textAnswer)
    {
        bool correct = Type switch
        {
            QuestionType.Essay => false,
            QuestionType.ShortAnswer => MatchesAcceptedAnswer(textAnswer),
            QuestionType.MultipleResponse => MatchesCorrectSet(selectedOptionIds),
            _ => selectedOptionIds.Count == 1
                && _options.Any(o => o.Id == selectedOptionIds.First() && o.IsCorrect)
        };

        return correct ? Points : 0;
    }

    /// <summary>Every selected option is correct, and no correct option was missed.</summary>
    private bool MatchesCorrectSet(IReadOnlyCollection<Guid> selectedOptionIds)
    {
        if (selectedOptionIds.Count == 0)
        {
            return false;
        }

        HashSet<Guid> correct = _options.Where(o => o.IsCorrect).Select(o => o.Id).ToHashSet();

        // Guards against an option id from another question being passed in.
        HashSet<Guid> selected = selectedOptionIds
            .Where(id => _options.Any(o => o.Id == id))
            .ToHashSet();

        return correct.Count > 0 && correct.SetEquals(selected);
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
        // Only a short-answer question is marked by comparing text, so a key on any other type
        // would be dead data that the author cannot see.
        if (acceptedAnswers is null || Type != QuestionType.ShortAnswer)
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

    private void SetMarkingGuidance(string? markingGuidance)
    {
        MarkingGuidance = Type == QuestionType.Essay && !string.IsNullOrWhiteSpace(markingGuidance)
            ? markingGuidance.Trim()
            : null;
    }

    private static int ClampPoints(int points) => Math.Clamp(points, 1, MaxPointsCeiling);
}
