namespace NovaLearn.Domain.Enrollments;

/// <summary>Lifecycle state of a student's enrolment in a course.</summary>
public enum EnrollmentStatus
{
    /// <summary>Currently studying; progress may still change.</summary>
    Active,

    /// <summary>Reached 100% progress.</summary>
    Completed,

    /// <summary>Withdrawn by the student (or an administrator).</summary>
    Dropped
}
