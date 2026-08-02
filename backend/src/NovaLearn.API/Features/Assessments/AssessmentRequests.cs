using NovaLearn.Domain.Assessments;

namespace NovaLearn.API.Features.Assessments;

/// <summary>Body for creating an assignment. Enums accept their string names, e.g. "Published".</summary>
public sealed record CreateAssignmentRequest(
    string Title,
    string? Instructions,
    DateTimeOffset? DueAtUtc,
    int MaxPoints,
    bool AllowLateSubmissions,
    AssessmentStatus Status);

/// <summary>Body for editing an assignment. Replaces every field.</summary>
public sealed record UpdateAssignmentRequest(
    string Title,
    string? Instructions,
    DateTimeOffset? DueAtUtc,
    int MaxPoints,
    bool AllowLateSubmissions,
    AssessmentStatus Status);

/// <summary>Body for handing work in. Submitting again replaces the previous attempt.</summary>
public sealed record SubmitAssignmentRequest(string Content, string? AttachmentUrl);

/// <summary>Body for marking a submission.</summary>
public sealed record GradeSubmissionRequest(int PointsAwarded, string? Feedback);
