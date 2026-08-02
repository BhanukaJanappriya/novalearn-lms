namespace NovaLearn.Application.Features.Assessments.Common;

/// <summary>
/// The marking grid for one course: every enrolled learner against every published assignment,
/// with the totals a lecturer would otherwise work out by hand.
/// </summary>
public sealed record GradebookDto(
    Guid CourseId,
    string CourseTitle,
    string CourseCode,
    int TotalPointsAvailable,
    IReadOnlyList<GradebookColumnDto> Assignments,
    IReadOnlyList<GradebookRowDto> Rows,
    GradebookSummaryDto Summary);

/// <summary>One assignment column, with how the cohort did on it.</summary>
public sealed record GradebookColumnDto(
    Guid AssignmentId,
    string Title,
    int MaxPoints,
    DateTimeOffset? DueAtUtc,
    int SubmittedCount,
    int GradedCount,
    double? AveragePoints);

/// <summary>One learner row, with a cell per assignment and their running total.</summary>
public sealed record GradebookRowDto(
    Guid StudentId,
    string StudentName,
    string StudentEmail,
    IReadOnlyList<GradebookCellDto> Cells,
    int PointsAwarded,
    int PointsGraded,
    double? PercentageOfGraded,
    int MissingCount);

/// <summary>
/// One learner's standing on one assignment. <see cref="Status"/> is the display state:
/// Missing, Submitted or Graded.
/// </summary>
public sealed record GradebookCellDto(
    Guid AssignmentId,
    Guid? SubmissionId,
    string Status,
    int? PointsAwarded,
    bool IsLate);

/// <summary>Cohort level figures for the header strip.</summary>
public sealed record GradebookSummaryDto(
    int StudentCount,
    int AssignmentCount,
    int AwaitingMarking,
    double? CohortAveragePercentage);
