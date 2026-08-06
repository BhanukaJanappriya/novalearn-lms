using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Common;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Quizzes.Events;

namespace NovaLearn.Domain.Quizzes;

/// <summary>
/// A self-marking test attached to a course, and the aggregate root for its
/// <see cref="Question"/> children. Reuses <see cref="AssessmentStatus"/> so publication means
/// the same thing here as it does for an assignment.
/// </summary>
public sealed class Quiz : BaseEntity
{
    /// <summary>Longest a quiz may run, so a stray value cannot leave an attempt open for years.</summary>
    public const int MaxTimeLimitMinutes = 600;

    private readonly List<Question> _questions = [];

    private Quiz() { } // EF Core

    public Guid CourseId { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public AssessmentStatus Status { get; private set; }

    /// <summary>Minutes allowed once started. Null means untimed.</summary>
    public int? TimeLimitMinutes { get; private set; }

    /// <summary>How many times a learner may sit it. Null means unlimited.</summary>
    public int? MaxAttempts { get; private set; }

    /// <summary>Percentage needed to pass. Null means the quiz is not pass or fail.</summary>
    public int? PassingScorePercent { get; private set; }

    /// <summary>Whether question order is randomised per attempt.</summary>
    public bool ShuffleQuestions { get; private set; }

    public Course? Course { get; private set; }

    /// <summary>The quiz's questions. Mutate through <see cref="AddQuestion"/>.</summary>
    public IReadOnlyCollection<Question> Questions => _questions.AsReadOnly();

    /// <summary>Everything a perfect attempt would score.</summary>
    public int TotalPoints => _questions.Sum(q => q.Points);

    public static Quiz Create(
        Guid courseId,
        string title,
        string? description,
        int? timeLimitMinutes,
        int? maxAttempts,
        int? passingScorePercent,
        bool shuffleQuestions,
        AssessmentStatus status) =>
        new()
        {
            CourseId = courseId,
            Title = title.Trim(),
            Description = Normalise(description),
            TimeLimitMinutes = ClampTimeLimit(timeLimitMinutes),
            MaxAttempts = ClampAttempts(maxAttempts),
            PassingScorePercent = ClampPercent(passingScorePercent),
            ShuffleQuestions = shuffleQuestions,
            Status = status
        };

    /// <summary>Applies edited details, keeping the same invariants as <see cref="Create"/>.</summary>
    public void Update(
        string title,
        string? description,
        int? timeLimitMinutes,
        int? maxAttempts,
        int? passingScorePercent,
        bool shuffleQuestions,
        AssessmentStatus status)
    {
        Title = title.Trim();
        Description = Normalise(description);
        TimeLimitMinutes = ClampTimeLimit(timeLimitMinutes);
        MaxAttempts = ClampAttempts(maxAttempts);
        PassingScorePercent = ClampPercent(passingScorePercent);
        ShuffleQuestions = shuffleQuestions;

        SetStatus(status);
    }

    public void Publish() => SetStatus(AssessmentStatus.Published);

    public void Unpublish() => Status = AssessmentStatus.Draft;

    /// <summary>
    /// Announces the quiz only as it crosses into Published, so editing a live quiz does not
    /// notify the whole cohort again.
    /// </summary>
    private void SetStatus(AssessmentStatus status)
    {
        bool isBecomingVisible = status == AssessmentStatus.Published && Status != AssessmentStatus.Published;

        Status = status;

        if (isBecomingVisible)
        {
            RaiseDomainEvent(new QuizPublishedDomainEvent(
                Id, CourseId, Title, _questions.Count, TimeLimitMinutes));
        }
    }

    public Question AddQuestion(
        string text,
        QuestionType type,
        int points,
        int sortOrder,
        IEnumerable<string>? acceptedAnswers,
        bool isRequired = false,
        string? markingGuidance = null)
    {
        Question question = Question.Create(
            Id, text, type, points, sortOrder, acceptedAnswers, isRequired, markingGuidance);

        _questions.Add(question);
        return question;
    }

    /// <summary>Whether any question on this quiz has to be marked by a person.</summary>
    public bool HasManuallyMarkedQuestions => _questions.Any(q => q.RequiresManualMarking);

    /// <summary>
    /// Whether the quiz can be sat: it needs at least one question, and every question must be
    /// complete. Publishing a quiz with a broken question would give learners something
    /// unanswerable that still counts against them.
    /// </summary>
    public bool IsReadyToPublish() =>
        _questions.Count > 0 && _questions.All(q => q.IsAnswerable());

    /// <summary>When an attempt started at <paramref name="startedAtUtc"/> runs out, if ever.</summary>
    public DateTimeOffset? DeadlineFor(DateTimeOffset startedAtUtc) =>
        TimeLimitMinutes is { } minutes ? startedAtUtc.AddMinutes(minutes) : null;

    /// <summary>Whether a learner who has already used <paramref name="used"/> attempts may sit it again.</summary>
    public bool AllowsAnotherAttempt(int used) => MaxAttempts is not { } max || used < max;

    /// <summary>Whether <paramref name="scorePercent"/> is a pass. False when no pass mark is set.</summary>
    public bool IsPass(double scorePercent) =>
        PassingScorePercent is { } required && scorePercent >= required;

    private static string? Normalise(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int? ClampTimeLimit(int? minutes) =>
        minutes is null ? null : Math.Clamp(minutes.Value, 1, MaxTimeLimitMinutes);

    private static int? ClampAttempts(int? attempts) =>
        attempts is null ? null : Math.Max(1, attempts.Value);

    private static int? ClampPercent(int? percent) =>
        percent is null ? null : Math.Clamp(percent.Value, 0, 100);
}
