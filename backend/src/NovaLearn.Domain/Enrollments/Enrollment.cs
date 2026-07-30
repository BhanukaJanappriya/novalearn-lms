using NovaLearn.Domain.Common;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Domain.Enrollments;

/// <summary>
/// A student's enrolment in a course. Auditing and soft-delete come from <see cref="BaseEntity"/>.
/// Constructed through <see cref="Create"/> and mutated only through the behaviour methods below,
/// so the progress range (0..100) and the progress/status/completion relationship always hold.
/// </summary>
public sealed class Enrollment : BaseEntity
{
    /// <summary>Highest possible progress value; reaching it completes the enrolment.</summary>
    public const int MaxProgressPercent = 100;

    private Enrollment() { } // EF Core

    public Guid StudentId { get; private set; }

    public Guid CourseId { get; private set; }

    public EnrollmentStatus Status { get; private set; }

    public DateTimeOffset EnrolledAtUtc { get; private set; }

    /// <summary>Completion percentage, always within 0..100.</summary>
    public int ProgressPercent { get; private set; }

    /// <summary>Set the moment progress first reaches 100%; cleared if the enrolment reopens.</summary>
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    /// <summary>Optional navigation to the enrolled student (for read projections).</summary>
    public ApplicationUser? Student { get; private set; }

    /// <summary>Optional navigation to the course (for read projections).</summary>
    public Course? Course { get; private set; }

    public static Enrollment Create(Guid studentId, Guid courseId, DateTimeOffset enrolledAtUtc)
    {
        return new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            Status = EnrollmentStatus.Active,
            EnrolledAtUtc = enrolledAtUtc,
            ProgressPercent = 0,
            CompletedAtUtc = null
        };
    }

    /// <summary>
    /// Records progress, clamping the input to 0..100. Reaching 100% completes the enrolment
    /// (stamping <see cref="CompletedAtUtc"/>); falling back below 100% reopens it.
    /// </summary>
    public void UpdateProgress(int percent, DateTimeOffset? completedAtUtc = null)
    {
        ProgressPercent = Math.Clamp(percent, 0, MaxProgressPercent);

        if (ProgressPercent == MaxProgressPercent)
        {
            Status = EnrollmentStatus.Completed;
            CompletedAtUtc ??= completedAtUtc ?? DateTimeOffset.UtcNow;
            return;
        }

        // Below 100% the enrolment is studying again, unless the student has dropped out.
        if (Status == EnrollmentStatus.Completed)
        {
            Status = EnrollmentStatus.Active;
        }

        CompletedAtUtc = null;
    }

    /// <summary>Withdraws the student from the course, keeping the progress recorded so far.</summary>
    public void Drop() => Status = EnrollmentStatus.Dropped;

    /// <summary>Puts a dropped (or completed) enrolment back into study.</summary>
    public void Reactivate()
    {
        Status = ProgressPercent == MaxProgressPercent ? EnrollmentStatus.Completed : EnrollmentStatus.Active;
    }
}
