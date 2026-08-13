using NovaLearn.Domain.Assessments;

namespace NovaLearn.Application.Common.Models;

/// <summary>Which of the two kinds of assessed work a row describes.</summary>
public enum AssessmentKind
{
    /// <summary>An assignment, handed in and marked by a person.</summary>
    Assignment,

    /// <summary>A quiz, auto marked except for essay answers.</summary>
    Quiz
}

/// <summary>
/// One piece of assessed work, flattened across courses so staff can see everything they are
/// responsible for in one list rather than one course at a time.
///
/// Counts are deliberately aggregate. This row says twelve people are waiting to be marked, never
/// who they are or what they scored: naming them is the per assignment and per quiz screens' job,
/// which already enforce course ownership on the way in.
/// </summary>
public sealed record AssessmentOverviewRow(
    AssessmentKind Kind,
    Guid Id,
    Guid CourseId,
    string CourseTitle,
    string Title,
    AssessmentStatus Status,
    DateTimeOffset? DueAtUtc,
    int MaxPoints,
    int QuestionCount,
    int EnrolledCount,
    int SubmittedCount,
    int AwaitingMarkingCount,
    int GradedCount,
    double? AverageScorePercent);

/// <summary>
/// Totals across every row the caller can see, computed on the server so two clients cannot
/// disagree about how much work is outstanding.
/// </summary>
public sealed record AssessmentOverviewSummary(
    int Total,
    int Published,
    int Drafts,
    int AwaitingMarking,
    int DueSoon,
    int Overdue);
