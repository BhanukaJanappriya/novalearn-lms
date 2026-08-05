using NovaLearn.Domain.Assessments.Events;
using NovaLearn.Domain.Common;
using NovaLearn.Domain.Courses;

namespace NovaLearn.Domain.Assessments;

/// <summary>
/// A piece of assessed work attached to a course. Auditing and soft-delete come from
/// <see cref="BaseEntity"/>. Constructed through <see cref="Create"/> so the invariants
/// (trimmed text, positive points ceiling) hold from the start.
/// </summary>
public sealed class Assignment : BaseEntity
{
    /// <summary>Upper bound on <see cref="MaxPoints"/>, keeping grades on a sane scale.</summary>
    public const int MaxPointsCeiling = 1000;

    private Assignment() { } // EF Core

    public Guid CourseId { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Instructions { get; private set; }

    /// <summary>When the work is due. Null means open ended, so nothing is ever late.</summary>
    public DateTimeOffset? DueAtUtc { get; private set; }

    /// <summary>Points a perfect submission earns. Always at least 1.</summary>
    public int MaxPoints { get; private set; }

    /// <summary>Whether work handed in after <see cref="DueAtUtc"/> is accepted (and flagged).</summary>
    public bool AllowLateSubmissions { get; private set; }

    public AssessmentStatus Status { get; private set; }

    /// <summary>Optional navigation to the owning course (for read projections and ownership checks).</summary>
    public Course? Course { get; private set; }

    public static Assignment Create(
        Guid courseId,
        string title,
        string? instructions,
        DateTimeOffset? dueAtUtc,
        int maxPoints,
        bool allowLateSubmissions,
        AssessmentStatus status)
    {
        var assignment = new Assignment
        {
            CourseId = courseId,
            Title = title.Trim(),
            Instructions = Normalise(instructions),
            DueAtUtc = dueAtUtc,
            MaxPoints = ClampPoints(maxPoints),
            AllowLateSubmissions = allowLateSubmissions,
            Status = status
        };

        // An assignment created straight into Published announces itself too.
        if (status == AssessmentStatus.Published)
        {
            assignment.RaiseDomainEvent(new AssignmentPublishedDomainEvent(
                assignment.Id, courseId, assignment.Title, dueAtUtc));
        }

        return assignment;
    }

    /// <summary>Applies edited details, keeping the same invariants as <see cref="Create"/>.</summary>
    public void Update(
        string title,
        string? instructions,
        DateTimeOffset? dueAtUtc,
        int maxPoints,
        bool allowLateSubmissions,
        AssessmentStatus status)
    {
        Title = title.Trim();
        Instructions = Normalise(instructions);
        DueAtUtc = dueAtUtc;
        MaxPoints = ClampPoints(maxPoints);
        AllowLateSubmissions = allowLateSubmissions;

        SetStatus(status);
    }

    public void Publish() => SetStatus(AssessmentStatus.Published);

    public void Unpublish() => Status = AssessmentStatus.Draft;

    /// <summary>
    /// Announces the assignment only as it crosses into Published. Raising on every save would
    /// notify the whole cohort each time a typo was fixed.
    /// </summary>
    private void SetStatus(AssessmentStatus status)
    {
        bool isBecomingVisible = status == AssessmentStatus.Published && Status != AssessmentStatus.Published;

        Status = status;

        if (isBecomingVisible)
        {
            RaiseDomainEvent(new AssignmentPublishedDomainEvent(Id, CourseId, Title, DueAtUtc));
        }
    }

    /// <summary>Whether work handed in at <paramref name="at"/> counts as late.</summary>
    public bool IsLateAt(DateTimeOffset at) => DueAtUtc is { } due && at > due;

    /// <summary>
    /// Whether a learner may hand work in at <paramref name="at"/>. Draft assignments are
    /// closed to everyone; past the due date it depends on <see cref="AllowLateSubmissions"/>.
    /// </summary>
    public bool AcceptsSubmissionAt(DateTimeOffset at) =>
        Status == AssessmentStatus.Published && (!IsLateAt(at) || AllowLateSubmissions);

    private static int ClampPoints(int points) => Math.Clamp(points, 1, MaxPointsCeiling);

    private static string? Normalise(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
