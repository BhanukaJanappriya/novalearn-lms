using NovaLearn.Domain.Common;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Domain.Assessments;

/// <summary>
/// One learner's answer to an <see cref="Assignment"/>. There is at most one live submission
/// per learner per assignment; handing in again replaces the content and clears any mark, so a
/// learner can never keep a grade that was awarded for different work.
/// </summary>
public sealed class Submission : BaseEntity
{
    private Submission() { } // EF Core

    public Guid AssignmentId { get; private set; }

    public Guid StudentId { get; private set; }

    /// <summary>The written answer. Always present; an attachment alone is not a submission.</summary>
    public string Content { get; private set; } = null!;

    /// <summary>Optional link to work hosted elsewhere, until file storage exists.</summary>
    public string? AttachmentUrl { get; private set; }

    public DateTimeOffset SubmittedAtUtc { get; private set; }

    /// <summary>Captured at hand-in time, so later edits to the due date do not rewrite history.</summary>
    public bool IsLate { get; private set; }

    public SubmissionStatus Status { get; private set; }

    public int? PointsAwarded { get; private set; }

    public string? Feedback { get; private set; }

    public Guid? GradedById { get; private set; }

    public DateTimeOffset? GradedAtUtc { get; private set; }

    public Assignment? Assignment { get; private set; }

    public ApplicationUser? Student { get; private set; }

    public static Submission Create(
        Guid assignmentId,
        Guid studentId,
        string content,
        string? attachmentUrl,
        DateTimeOffset submittedAtUtc,
        bool isLate)
    {
        return new Submission
        {
            AssignmentId = assignmentId,
            StudentId = studentId,
            Content = content.Trim(),
            AttachmentUrl = Normalise(attachmentUrl),
            SubmittedAtUtc = submittedAtUtc,
            IsLate = isLate,
            Status = SubmissionStatus.Submitted
        };
    }

    /// <summary>
    /// Replaces the submitted work. Any existing mark is discarded, because it was awarded for
    /// content that no longer exists.
    /// </summary>
    public void Resubmit(string content, string? attachmentUrl, DateTimeOffset submittedAtUtc, bool isLate)
    {
        Content = content.Trim();
        AttachmentUrl = Normalise(attachmentUrl);
        SubmittedAtUtc = submittedAtUtc;
        IsLate = isLate;

        Status = SubmissionStatus.Submitted;
        PointsAwarded = null;
        Feedback = null;
        GradedById = null;
        GradedAtUtc = null;
    }

    /// <summary>
    /// Records a mark. Points are clamped to 0..<paramref name="maxPoints"/> so a grader cannot
    /// award more than the assignment is worth.
    /// </summary>
    public void Grade(int points, string? feedback, int maxPoints, Guid gradedById, DateTimeOffset gradedAtUtc)
    {
        PointsAwarded = Math.Clamp(points, 0, maxPoints);
        Feedback = Normalise(feedback);
        GradedById = gradedById;
        GradedAtUtc = gradedAtUtc;
        Status = SubmissionStatus.Graded;
    }

    private static string? Normalise(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
