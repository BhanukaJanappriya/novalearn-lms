using NovaLearn.Domain.Assessments;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>
/// Persistence port for the <see cref="Assignment"/> aggregate and its submissions.
/// Submissions live here rather than on their own port because every use case that touches one
/// also needs its assignment (for the points ceiling and the owning course).
/// </summary>
public interface IAssessmentRepository
{
    Task AddAssignmentAsync(Assignment assignment, CancellationToken cancellationToken);

    /// <summary>Loads an assignment with its course, so ownership can be checked in one round trip.</summary>
    Task<Assignment?> GetAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken);

    /// <summary>A course's assignments, earliest due date first, undated last.</summary>
    Task<IReadOnlyList<Assignment>> ListAssignmentsAsync(Guid courseId, CancellationToken cancellationToken);

    void RemoveAssignment(Assignment assignment);

    /// <summary>
    /// Tracks a submission as an insert. <see cref="Domain.Common.BaseEntity"/> assigns the key
    /// client-side, so an entity reached only through a navigation is tracked as Modified and
    /// saves as a no-op UPDATE. The insert has to be stated explicitly.
    /// </summary>
    Task AddSubmissionAsync(Submission submission, CancellationToken cancellationToken);

    /// <summary>Loads a submission with its assignment, that assignment's course, and the learner.</summary>
    Task<Submission?> GetSubmissionAsync(Guid submissionId, CancellationToken cancellationToken);

    /// <summary>A learner's live submission for an assignment, if any.</summary>
    Task<Submission?> GetSubmissionForStudentAsync(
        Guid assignmentId, Guid studentId, CancellationToken cancellationToken);

    /// <summary>Every submission for an assignment, newest first, with the learner included.</summary>
    Task<IReadOnlyList<Submission>> ListSubmissionsAsync(Guid assignmentId, CancellationToken cancellationToken);

    /// <summary>
    /// A learner's submissions across every assignment in one course, each with its assignment
    /// loaded so titles and points ceilings project without a second round trip.
    /// </summary>
    Task<IReadOnlyList<Submission>> ListSubmissionsForStudentAsync(
        Guid courseId, Guid studentId, CancellationToken cancellationToken);

    /// <summary>
    /// Submission tallies for every assignment in a course, in one query. Avoids walking the
    /// assignments and counting each one separately.
    /// </summary>
    Task<IReadOnlyList<SubmissionTally>> TallySubmissionsAsync(
        Guid courseId, CancellationToken cancellationToken);
}

/// <summary>How many submissions an assignment has, and how many of those are marked.</summary>
public sealed record SubmissionTally(Guid AssignmentId, int Total, int Graded);
