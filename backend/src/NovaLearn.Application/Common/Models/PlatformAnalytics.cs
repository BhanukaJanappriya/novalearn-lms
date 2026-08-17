namespace NovaLearn.Application.Common.Models;

/// <summary>
/// A figure for the chosen window alongside the same figure for the window before it.
///
/// Analytics is about direction, not just size, so the comparison is computed on the server and
/// travels with the number. Two clients cannot then disagree about whether something is improving.
/// </summary>
public sealed record TrendMetric(double Current, double Previous)
{
    /// <summary>
    /// Change against the previous window, as a percentage.
    ///
    /// Null when the previous window was empty: everything is an infinite improvement on nothing,
    /// which is worth saying plainly rather than rendering as a triumphant number.
    /// </summary>
    public double? ChangePercent =>
        Previous <= 0 ? null : Math.Round((Current - Previous) / Previous * 100, 1);
}

/// <summary>How the series is bucketed, which follows from how long the window is.</summary>
public enum AnalyticsGranularity
{
    Day,
    Week,
    Month
}

/// <summary>The period the figures cover.</summary>
public sealed record AnalyticsWindow(
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc,
    int Days,
    AnalyticsGranularity Granularity);

/// <summary>The four figures at the top of the page.</summary>
public sealed record AnalyticsHeadline(
    TrendMetric Enrolments,
    TrendMetric Completions,
    TrendMetric ActiveLearners,
    TrendMetric AverageScorePercent);

/// <summary>One bucket on the enrolments and completions chart.</summary>
public sealed record AnalyticsPoint(DateOnly Date, int Enrolments, int Completions);

/// <summary>
/// How one course is doing.
///
/// <paramref name="AtRiskCount"/> is learners still active on the course who have been enrolled
/// for over a fortnight and are under a quarter of the way through. It is a rule of thumb for
/// which courses need a look, not a judgement about any individual, and nobody is named here.
/// </summary>
public sealed record CoursePerformanceRow(
    Guid CourseId,
    string Title,
    Guid? DepartmentId,
    string? DepartmentName,
    int Enrolled,
    int Completed,
    double CompletionRatePercent,
    double AverageProgressPercent,
    double? AverageScorePercent,
    int AtRiskCount);

/// <summary>How one department is doing, or the courses belonging to none.</summary>
public sealed record DepartmentPerformanceRow(
    Guid? DepartmentId,
    string Name,
    int Courses,
    int Enrolled,
    int Completed,
    double CompletionRatePercent);

/// <summary>A band of marks and how many results landed in it.</summary>
public sealed record ScoreBucket(string Label, int Count);

/// <summary>Everything the analytics page shows, computed for one window.</summary>
public sealed record PlatformAnalytics(
    AnalyticsWindow Window,
    AnalyticsHeadline Headline,
    IReadOnlyList<AnalyticsPoint> Series,
    IReadOnlyList<CoursePerformanceRow> Courses,
    IReadOnlyList<DepartmentPerformanceRow> Departments,
    IReadOnlyList<ScoreBucket> ScoreDistribution);
