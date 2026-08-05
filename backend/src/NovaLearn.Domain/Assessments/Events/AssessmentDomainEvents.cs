using NovaLearn.Domain.Common;

namespace NovaLearn.Domain.Assessments.Events;

/// <summary>
/// Raised when an assignment first becomes visible to learners. Only on the transition, so
/// editing an already-published assignment does not notify everyone again.
/// </summary>
public sealed record AssignmentPublishedDomainEvent(
    Guid AssignmentId,
    Guid CourseId,
    string Title,
    DateTimeOffset? DueAtUtc) : DomainEvent;

/// <summary>Raised when a learner hands work in, so the course owner knows there is marking to do.</summary>
public sealed record SubmissionReceivedDomainEvent(
    Guid SubmissionId,
    Guid AssignmentId,
    Guid StudentId,
    bool IsLate) : DomainEvent;

/// <summary>Raised when a submission is marked, so the learner learns their result.</summary>
public sealed record SubmissionGradedDomainEvent(
    Guid SubmissionId,
    Guid AssignmentId,
    Guid StudentId,
    int PointsAwarded,
    int MaxPoints) : DomainEvent;
