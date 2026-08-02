using NovaLearn.Domain.Assessments;

namespace NovaLearn.Application.Features.Assessments.Common;

/// <summary>
/// An assignment as the client sees it. <see cref="MySubmission"/> is populated for learners
/// and left null for staff, whose view of who submitted what is the roster instead.
/// </summary>
public sealed record AssignmentDto(
    Guid Id,
    Guid CourseId,
    string Title,
    string? Instructions,
    DateTimeOffset? DueAtUtc,
    int MaxPoints,
    bool AllowLateSubmissions,
    string Status,
    bool IsOpen,
    int SubmissionCount,
    int GradedCount,
    SubmissionDto? MySubmission)
{
    public static AssignmentDto FromEntity(
        Assignment assignment,
        DateTimeOffset now,
        int submissionCount = 0,
        int gradedCount = 0,
        SubmissionDto? mySubmission = null) => new(
        assignment.Id,
        assignment.CourseId,
        assignment.Title,
        assignment.Instructions,
        assignment.DueAtUtc,
        assignment.MaxPoints,
        assignment.AllowLateSubmissions,
        assignment.Status.ToString(),
        assignment.AcceptsSubmissionAt(now),
        submissionCount,
        gradedCount,
        mySubmission);
}

/// <summary>A learner's submission, with the marking outcome when there is one.</summary>
public sealed record SubmissionDto(
    Guid Id,
    Guid AssignmentId,
    string AssignmentTitle,
    int MaxPoints,
    Guid StudentId,
    string StudentName,
    string StudentEmail,
    string Content,
    string? AttachmentUrl,
    DateTimeOffset SubmittedAtUtc,
    bool IsLate,
    string Status,
    int? PointsAwarded,
    string? Feedback,
    DateTimeOffset? GradedAtUtc)
{
    public static SubmissionDto FromEntity(Submission submission) => new(
        submission.Id,
        submission.AssignmentId,
        submission.Assignment?.Title ?? string.Empty,
        submission.Assignment?.MaxPoints ?? 0,
        submission.StudentId,
        submission.Student?.FullName ?? "Unknown",
        submission.Student?.Email ?? string.Empty,
        submission.Content,
        submission.AttachmentUrl,
        submission.SubmittedAtUtc,
        submission.IsLate,
        submission.Status.ToString(),
        submission.PointsAwarded,
        submission.Feedback,
        submission.GradedAtUtc);
}
