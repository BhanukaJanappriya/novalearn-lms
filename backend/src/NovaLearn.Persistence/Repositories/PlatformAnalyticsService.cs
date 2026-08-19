using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Quizzes;

namespace NovaLearn.Persistence.Repositories;

/// <summary>
/// EF Core implementation of platform analytics.
///
/// Aggregates that group cleanly are done in SQL. Anything that needs bucketing by date is pulled
/// as a bare list of timestamps and bucketed in memory, following the precedent set by the admin
/// statistics service: date-part grouping is the provider-specific corner where this codebase's
/// queries have historically compiled and then failed at runtime.
/// </summary>
internal sealed class PlatformAnalyticsService(ApplicationDbContext context) : IPlatformAnalytics
{
    /// <summary>Enrolled this long without getting a quarter of the way through is a warning sign.</summary>
    private const int AtRiskAfterDays = 14;
    private const int AtRiskBelowPercent = 25;

    public async Task<PlatformAnalytics> GetAsync(int days, CancellationToken cancellationToken)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset from = now.AddDays(-days);

        // The window immediately before this one, of the same length, so the two are comparable.
        DateTimeOffset previousFrom = from.AddDays(-days);

        AnalyticsGranularity granularity = AnalyticsBucketing.GranularityFor(days);

        var window = new AnalyticsWindow(from, now, days, granularity);

        AnalyticsHeadline headline = await BuildHeadlineAsync(from, previousFrom, now, cancellationToken);
        IReadOnlyList<AnalyticsPoint> series = await BuildSeriesAsync(from, granularity, cancellationToken);
        IReadOnlyList<CoursePerformanceRow> courses = await BuildCoursesAsync(now, cancellationToken);
        IReadOnlyList<ScoreBucket> distribution = await BuildScoreDistributionAsync(from, cancellationToken);

        return new PlatformAnalytics(
            window, headline, series, courses, BuildDepartments(courses), distribution);
    }

    private async Task<AnalyticsHeadline> BuildHeadlineAsync(
        DateTimeOffset from,
        DateTimeOffset previousFrom,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        int enrolments = await context.Enrollments
            .CountAsync(e => e.EnrolledAtUtc >= from, cancellationToken);

        int enrolmentsBefore = await context.Enrollments
            .CountAsync(e => e.EnrolledAtUtc >= previousFrom && e.EnrolledAtUtc < from, cancellationToken);

        int completions = await context.Enrollments
            .CountAsync(e => e.CompletedAtUtc != null && e.CompletedAtUtc >= from, cancellationToken);

        int completionsBefore = await context.Enrollments
            .CountAsync(
                e => e.CompletedAtUtc != null && e.CompletedAtUtc >= previousFrom && e.CompletedAtUtc < from,
                cancellationToken);

        // "Learner" is the role, so a lecturer signing in does not inflate the figure.
        List<Guid> studentIds = await (
                from userRole in context.UserRoles
                join role in context.Roles on userRole.RoleId equals role.Id
                where role.Name == Roles.Student
                select userRole.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        int activeLearners = await context.Users
            .CountAsync(
                u => studentIds.Contains(u.Id) && u.LastLoginAtUtc != null && u.LastLoginAtUtc >= from,
                cancellationToken);

        int activeLearnersBefore = await context.Users
            .CountAsync(
                u => studentIds.Contains(u.Id)
                     && u.LastLoginAtUtc != null
                     && u.LastLoginAtUtc >= previousFrom
                     && u.LastLoginAtUtc < from,
                cancellationToken);

        double averageScore = await AverageScoreAsync(from, now, cancellationToken);
        double averageScoreBefore = await AverageScoreAsync(previousFrom, from, cancellationToken);

        return new AnalyticsHeadline(
            new TrendMetric(enrolments, enrolmentsBefore),
            new TrendMetric(completions, completionsBefore),
            new TrendMetric(activeLearners, activeLearnersBefore),
            new TrendMetric(averageScore, averageScoreBefore));
    }

    /// <summary>
    /// The average mark across everything graded in a period, assignments and quizzes together.
    ///
    /// Assignment marks are points out of a maximum and quiz marks are already percentages, so
    /// both are converted to percentages before being combined. Averaging the two averages would
    /// weight a quiz sat by three people the same as an assignment marked for ninety.
    /// </summary>
    private async Task<double> AverageScoreAsync(
        DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        List<double> submissionScores = await context.Submissions
            .AsNoTracking()
            .Where(s => s.Status == SubmissionStatus.Graded
                        && s.GradedAtUtc >= from
                        && s.GradedAtUtc < to
                        && s.PointsAwarded != null
                        && s.Assignment!.MaxPoints > 0)
            .Select(s => (double)s.PointsAwarded! / s.Assignment!.MaxPoints * 100)
            .ToListAsync(cancellationToken);

        List<double> attemptScores = await context.QuizAttempts
            .AsNoTracking()
            .Where(a => a.Status == AttemptStatus.Graded
                        && a.MarkedAtUtc >= from
                        && a.MarkedAtUtc < to)
            .Select(a => a.ScorePercent)
            .ToListAsync(cancellationToken);

        List<double> all = [.. submissionScores, .. attemptScores];

        return all.Count == 0 ? 0 : Math.Round(all.Average(), 1);
    }

    private async Task<IReadOnlyList<AnalyticsPoint>> BuildSeriesAsync(
        DateTimeOffset from, AnalyticsGranularity granularity, CancellationToken cancellationToken)
    {
        List<DateTimeOffset> enrolled = await context.Enrollments
            .AsNoTracking()
            .Where(e => e.EnrolledAtUtc >= from)
            .Select(e => e.EnrolledAtUtc)
            .ToListAsync(cancellationToken);

        List<DateTimeOffset> completed = await context.Enrollments
            .AsNoTracking()
            .Where(e => e.CompletedAtUtc != null && e.CompletedAtUtc >= from)
            .Select(e => e.CompletedAtUtc!.Value)
            .ToListAsync(cancellationToken);

        Dictionary<DateOnly, int> enrolmentsByBucket = AnalyticsBucketing.Count(enrolled, granularity);
        Dictionary<DateOnly, int> completionsByBucket = AnalyticsBucketing.Count(completed, granularity);

        return AnalyticsBucketing.AllBuckets(from, DateTimeOffset.UtcNow, granularity)
            .Select(bucket => new AnalyticsPoint(
                bucket,
                enrolmentsByBucket.GetValueOrDefault(bucket),
                completionsByBucket.GetValueOrDefault(bucket)))
            .ToList();
    }

    /// <summary>
    /// Course performance over the whole life of each course rather than the window.
    ///
    /// A completion rate is only meaningful against everyone who ever enrolled: restricting it to
    /// the last thirty days would compare this month's completions against this month's joiners
    /// and produce a number that swings wildly for reasons nobody can act on.
    /// </summary>
    private async Task<IReadOnlyList<CoursePerformanceRow>> BuildCoursesAsync(
        DateTimeOffset now, CancellationToken cancellationToken)
    {
        var courses = await context.Courses
            .AsNoTracking()
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.DepartmentId,
                DepartmentName = c.Department != null ? c.Department.Name : null
            })
            .ToListAsync(cancellationToken);

        if (courses.Count == 0)
        {
            return [];
        }

        DateTimeOffset atRiskCutoff = now.AddDays(-AtRiskAfterDays);

        var enrolmentStats = await context.Enrollments
            .AsNoTracking()
            .GroupBy(e => e.CourseId)
            .Select(g => new
            {
                CourseId = g.Key,
                Enrolled = g.Count(),
                Completed = g.Count(e => e.Status == EnrollmentStatus.Completed),
                AverageProgress = g.Average(e => (double)e.ProgressPercent),
                AtRisk = g.Count(e =>
                    e.Status == EnrollmentStatus.Active
                    && e.ProgressPercent < AtRiskBelowPercent
                    && e.EnrolledAtUtc < atRiskCutoff)
            })
            .ToDictionaryAsync(x => x.CourseId, cancellationToken);

        Dictionary<Guid, double> averageScores = await CourseAverageScoresAsync(cancellationToken);

        return courses
            .Select(course =>
            {
                enrolmentStats.TryGetValue(course.Id, out var stat);

                int enrolled = stat?.Enrolled ?? 0;
                int completed = stat?.Completed ?? 0;

                return new CoursePerformanceRow(
                    course.Id,
                    course.Title,
                    course.DepartmentId,
                    course.DepartmentName,
                    enrolled,
                    completed,
                    enrolled == 0 ? 0 : Math.Round((double)completed / enrolled * 100, 1),
                    Math.Round(stat?.AverageProgress ?? 0, 1),
                    averageScores.TryGetValue(course.Id, out double score) ? score : null,
                    stat?.AtRisk ?? 0);
            })
            .OrderByDescending(row => row.Enrolled)
            .ThenBy(row => row.Title)
            .ToList();
    }

    /// <summary>Average mark per course, combining assignment and quiz results.</summary>
    private async Task<Dictionary<Guid, double>> CourseAverageScoresAsync(
        CancellationToken cancellationToken)
    {
        var submissionScores = await context.Submissions
            .AsNoTracking()
            .Where(s => s.Status == SubmissionStatus.Graded
                        && s.PointsAwarded != null
                        && s.Assignment!.MaxPoints > 0)
            .Select(s => new
            {
                s.Assignment!.CourseId,
                Percent = (double)s.PointsAwarded! / s.Assignment.MaxPoints * 100
            })
            .ToListAsync(cancellationToken);

        var attemptScores = await context.QuizAttempts
            .AsNoTracking()
            .Where(a => a.Status == AttemptStatus.Graded)
            .Select(a => new { a.Quiz!.CourseId, Percent = a.ScorePercent })
            .ToListAsync(cancellationToken);

        return submissionScores
            .Concat(attemptScores)
            .GroupBy(row => row.CourseId)
            .ToDictionary(
                group => group.Key,
                group => Math.Round(group.Average(row => row.Percent), 1));
    }

    /// <summary>
    /// Rolls the course rows up by department, in memory, since they are already loaded and a
    /// department's figures are by definition the sum of its courses'.
    /// </summary>
    private static IReadOnlyList<DepartmentPerformanceRow> BuildDepartments(
        IReadOnlyList<CoursePerformanceRow> courses) =>
        courses
            .GroupBy(course => new { course.DepartmentId, course.DepartmentName })
            .Select(group =>
            {
                int enrolled = group.Sum(course => course.Enrolled);
                int completed = group.Sum(course => course.Completed);

                return new DepartmentPerformanceRow(
                    group.Key.DepartmentId,
                    group.Key.DepartmentName ?? "No department",
                    group.Count(),
                    enrolled,
                    completed,
                    enrolled == 0 ? 0 : Math.Round((double)completed / enrolled * 100, 1));
            })
            .OrderByDescending(row => row.Enrolled)
            .ThenBy(row => row.Name)
            .ToList();

    /// <summary>
    /// How marks awarded in the window were distributed. Every band is returned even when empty,
    /// so the shape of the chart does not change depending on how the term is going.
    /// </summary>
    private async Task<IReadOnlyList<ScoreBucket>> BuildScoreDistributionAsync(
        DateTimeOffset from, CancellationToken cancellationToken)
    {
        List<double> submissionScores = await context.Submissions
            .AsNoTracking()
            .Where(s => s.Status == SubmissionStatus.Graded
                        && s.GradedAtUtc >= from
                        && s.PointsAwarded != null
                        && s.Assignment!.MaxPoints > 0)
            .Select(s => (double)s.PointsAwarded! / s.Assignment!.MaxPoints * 100)
            .ToListAsync(cancellationToken);

        List<double> attemptScores = await context.QuizAttempts
            .AsNoTracking()
            .Where(a => a.Status == AttemptStatus.Graded && a.MarkedAtUtc >= from)
            .Select(a => a.ScorePercent)
            .ToListAsync(cancellationToken);

        List<double> all = [.. submissionScores, .. attemptScores];

        (string Label, Func<double, bool> Matches)[] bands =
        [
            ("Under 50", score => score < 50),
            ("50 to 59", score => score is >= 50 and < 60),
            ("60 to 69", score => score is >= 60 and < 70),
            ("70 to 79", score => score is >= 70 and < 80),
            ("80 to 89", score => score is >= 80 and < 90),
            ("90 plus", score => score >= 90)
        ];

        return bands
            .Select(band => new ScoreBucket(band.Label, all.Count(band.Matches)))
            .ToList();
    }
}
