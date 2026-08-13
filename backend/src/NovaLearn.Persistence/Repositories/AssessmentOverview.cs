using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Quizzes;

namespace NovaLearn.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the cross course assessment list.
///
/// Follows the same shape as the other read models here: a handful of flat queries, each grouped
/// on the server, stitched together in memory. Aggregates live in the GROUP BY rather than in a
/// subquery inside a projection, which is the pattern that has repeatedly compiled and then thrown
/// at runtime in this codebase.
/// </summary>
internal sealed class AssessmentOverview(ApplicationDbContext context) : IAssessmentOverview
{
    public async Task<IReadOnlyList<AssessmentOverviewRow>> ListAsync(
        Guid? lecturerId, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Courses.Course> courses = context.Courses.AsNoTracking();

        if (lecturerId is { } ownerId)
        {
            courses = courses.Where(course => course.LecturerId == ownerId);
        }

        var courseRows = await courses
            .Select(course => new { course.Id, course.Title })
            .ToListAsync(cancellationToken);

        if (courseRows.Count == 0)
        {
            return [];
        }

        List<Guid> courseIds = courseRows.Select(course => course.Id).ToList();
        Dictionary<Guid, string> courseTitles = courseRows.ToDictionary(c => c.Id, c => c.Title);

        // Only active enrolments count as "expected to hand in". Someone who dropped the course
        // is not outstanding work, and counting them would make every assignment look unfinished.
        Dictionary<Guid, int> enrolled = await context.Enrollments
            .AsNoTracking()
            .Where(e => courseIds.Contains(e.CourseId) && e.Status != EnrollmentStatus.Dropped)
            .GroupBy(e => e.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.Count, cancellationToken);

        List<AssessmentOverviewRow> rows =
        [
            .. await BuildAssignmentRowsAsync(courseIds, courseTitles, enrolled, cancellationToken),
            .. await BuildQuizRowsAsync(courseIds, courseTitles, enrolled, cancellationToken),
        ];

        // Anything waiting on a person first, then by deadline, then by title. Undated work sorts
        // after dated work rather than ahead of it, since a quiz with no deadline is rarely the
        // most urgent thing on the list.
        return rows
            .OrderByDescending(row => row.AwaitingMarkingCount > 0)
            .ThenBy(row => row.DueAtUtc is null)
            .ThenBy(row => row.DueAtUtc)
            .ThenBy(row => row.Title)
            .ToList();
    }

    private async Task<List<AssessmentOverviewRow>> BuildAssignmentRowsAsync(
        List<Guid> courseIds,
        Dictionary<Guid, string> courseTitles,
        Dictionary<Guid, int> enrolled,
        CancellationToken cancellationToken)
    {
        var assignments = await context.Assignments
            .AsNoTracking()
            .Where(a => courseIds.Contains(a.CourseId))
            .Select(a => new
            {
                a.Id,
                a.CourseId,
                a.Title,
                a.Status,
                a.DueAtUtc,
                a.MaxPoints,
            })
            .ToListAsync(cancellationToken);

        if (assignments.Count == 0)
        {
            return [];
        }

        List<Guid> assignmentIds = assignments.Select(a => a.Id).ToList();

        var stats = await context.Submissions
            .AsNoTracking()
            .Where(s => assignmentIds.Contains(s.AssignmentId))
            .GroupBy(s => s.AssignmentId)
            .Select(g => new
            {
                AssignmentId = g.Key,
                Submitted = g.Count(),
                Awaiting = g.Count(s => s.Status == SubmissionStatus.Submitted),
                Graded = g.Count(s => s.Status == SubmissionStatus.Graded),

                // A conditional inside AVG rather than a filtered subquery: SQL ignores the nulls,
                // so ungraded work does not drag the average towards zero.
                AveragePoints = g.Average(s =>
                    s.Status == SubmissionStatus.Graded ? (double?)s.PointsAwarded : null),
            })
            .ToDictionaryAsync(x => x.AssignmentId, cancellationToken);

        return assignments
            .Select(a =>
            {
                stats.TryGetValue(a.Id, out var stat);

                return new AssessmentOverviewRow(
                    AssessmentKind.Assignment,
                    a.Id,
                    a.CourseId,
                    courseTitles.GetValueOrDefault(a.CourseId, "Unknown course"),
                    a.Title,
                    a.Status,
                    a.DueAtUtc,
                    a.MaxPoints,
                    QuestionCount: 0,
                    enrolled.GetValueOrDefault(a.CourseId),
                    stat?.Submitted ?? 0,
                    stat?.Awaiting ?? 0,
                    stat?.Graded ?? 0,
                    AsPercentOf(stat?.AveragePoints, a.MaxPoints));
            })
            .ToList();
    }

    private async Task<List<AssessmentOverviewRow>> BuildQuizRowsAsync(
        List<Guid> courseIds,
        Dictionary<Guid, string> courseTitles,
        Dictionary<Guid, int> enrolled,
        CancellationToken cancellationToken)
    {
        var quizzes = await context.Quizzes
            .AsNoTracking()
            .Where(q => courseIds.Contains(q.CourseId))
            .Select(q => new { q.Id, q.CourseId, q.Title, q.Status })
            .ToListAsync(cancellationToken);

        if (quizzes.Count == 0)
        {
            return [];
        }

        List<Guid> quizIds = quizzes.Select(q => q.Id).ToList();

        // Total points are the sum of the questions, so they come from the questions table rather
        // than from Quiz.TotalPoints, which is computed off a loaded collection.
        var shape = await context.QuizQuestions
            .AsNoTracking()
            .Where(q => quizIds.Contains(q.QuizId))
            .GroupBy(q => q.QuizId)
            .Select(g => new
            {
                QuizId = g.Key,
                Questions = g.Count(),
                Points = g.Sum(q => q.Points),
            })
            .ToDictionaryAsync(x => x.QuizId, cancellationToken);

        var stats = await context.QuizAttempts
            .AsNoTracking()
            .Where(a => quizIds.Contains(a.QuizId))
            .GroupBy(a => a.QuizId)
            .Select(g => new
            {
                QuizId = g.Key,

                // An attempt still in progress has not been handed in, so it is not a submission.
                Submitted = g.Count(a => a.Status != AttemptStatus.InProgress),
                Awaiting = g.Count(a => a.Status == AttemptStatus.PendingReview),
                Graded = g.Count(a => a.Status == AttemptStatus.Graded),
                AverageScore = g.Average(a =>
                    a.Status == AttemptStatus.Graded ? (double?)a.ScorePercent : null),
            })
            .ToDictionaryAsync(x => x.QuizId, cancellationToken);

        return quizzes
            .Select(q =>
            {
                shape.TryGetValue(q.Id, out var form);
                stats.TryGetValue(q.Id, out var stat);

                return new AssessmentOverviewRow(
                    AssessmentKind.Quiz,
                    q.Id,
                    q.CourseId,
                    courseTitles.GetValueOrDefault(q.CourseId, "Unknown course"),
                    q.Title,
                    q.Status,

                    // Quizzes have no deadline of their own yet; they are open while published.
                    DueAtUtc: null,
                    form?.Points ?? 0,
                    form?.Questions ?? 0,
                    enrolled.GetValueOrDefault(q.CourseId),
                    stat?.Submitted ?? 0,
                    stat?.Awaiting ?? 0,
                    stat?.Graded ?? 0,
                    Rounded(stat?.AverageScore));
            })
            .ToList();
    }

    /// <summary>Turns an average mark into a percentage, guarding the unmarked case.</summary>
    private static double? AsPercentOf(double? averagePoints, int maxPoints) =>
        averagePoints is null || maxPoints <= 0 ? null : Rounded(averagePoints.Value / maxPoints * 100);

    private static double? Rounded(double? value) =>
        value is null ? null : Math.Round(value.Value, 1);
}
