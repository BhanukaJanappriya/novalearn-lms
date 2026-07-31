using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Student.Dashboard;

/// <summary>
/// Shapes the learner read model into the dashboard contract. Unlike the admin dashboard, every
/// section here is backed by real data, so there is nothing synthetic to layer on.
/// </summary>
public sealed class GetStudentDashboardQueryHandler(
    IStudentDashboardService dashboard,
    ICurrentUser currentUser)
    : IRequestHandler<GetStudentDashboardQuery, Result<StudentDashboardResponse>>
{
    /// <summary>Progress at or above this is treated as "nearly done".</summary>
    private const int NearlyDoneThreshold = 75;

    /// <summary>How many months of enrolment activity the sparkline covers.</summary>
    private const int ActivityMonths = 6;

    private const string CompletedStatus = "Completed";

    private static readonly string[] MonthAbbrev =
        ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    public async Task<Result<StudentDashboardResponse>> Handle(
        GetStudentDashboardQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } studentId)
        {
            return Result.Failure<StudentDashboardResponse>(EnrollmentErrors.Unauthenticated);
        }

        StudentStatistics stats = await dashboard.GetForStudentAsync(studentId, cancellationToken);

        List<StudentEnrollmentRow> completed =
            stats.Enrollments.Where(e => e.Status == CompletedStatus).ToList();

        // Closest to the finish line first: that is the course the learner most likely wants next.
        List<StudentEnrollmentRow> inProgress = stats.Enrollments
            .Where(e => e.Status != CompletedStatus)
            .OrderByDescending(e => e.ProgressPercent)
            .ThenByDescending(e => e.EnrolledAtUtc)
            .ToList();

        return new StudentDashboardResponse(
            Summary: BuildSummary(stats, inProgress, completed),
            ContinueLearning: inProgress.Select(ToCourseDto).ToList(),
            Completed: completed
                .OrderByDescending(e => e.CompletedAtUtc ?? e.EnrolledAtUtc)
                .Select(ToCourseDto)
                .ToList(),
            CategoryProgress: BuildCategoryProgress(stats),
            EnrollmentActivity: BuildActivity(stats, DateTimeOffset.UtcNow),
            Recommended: stats.Recommended.Select(ToRecommendedDto).ToList());
    }

    private static StudentSummaryDto BuildSummary(
        StudentStatistics stats,
        IReadOnlyList<StudentEnrollmentRow> inProgress,
        IReadOnlyList<StudentEnrollmentRow> completed)
    {
        // Averaged across every non-dropped enrolment, so finishing a course raises the figure
        // rather than removing it from the denominator.
        int averageProgress = stats.Enrollments.Count == 0
            ? 0
            : (int)Math.Round(stats.Enrollments.Average(e => e.ProgressPercent));

        return new StudentSummaryDto(
            ActiveCourses: inProgress.Count,
            CompletedCourses: completed.Count,
            AverageProgressPercent: averageProgress,
            LessonsAvailable: stats.Enrollments.Sum(e => e.LessonCount),
            LearningMinutes: stats.Enrollments.Sum(e => e.TotalMinutes),
            CoursesNearlyDone: inProgress.Count(e => e.ProgressPercent >= NearlyDoneThreshold));
    }

    private static List<CategoryProgressDto> BuildCategoryProgress(StudentStatistics stats) =>
        stats.Enrollments
            .GroupBy(e => e.Category)
            .Select(g => new CategoryProgressDto(
                g.Key,
                g.Count(),
                (int)Math.Round(g.Average(e => e.ProgressPercent))))
            .OrderByDescending(c => c.CourseCount)
            .ThenBy(c => c.Label)
            .ToList();

    /// <summary>
    /// A dense trailing window: every month appears, including the ones with no enrolments, so
    /// the client can render a bar per month without filling gaps itself.
    /// </summary>
    private static List<ActivityPointDto> BuildActivity(StudentStatistics stats, DateTimeOffset now)
    {
        var points = new List<ActivityPointDto>(ActivityMonths);

        for (int offset = ActivityMonths - 1; offset >= 0; offset--)
        {
            DateTimeOffset month = now.AddMonths(-offset);
            int count = stats.MonthlyEnrollments
                .FirstOrDefault(m => m.Year == month.Year && m.Month == month.Month)?.Count ?? 0;

            points.Add(new ActivityPointDto(MonthAbbrev[month.Month - 1], count));
        }

        return points;
    }

    private static StudentCourseDto ToCourseDto(StudentEnrollmentRow row) => new(
        row.EnrollmentId,
        row.CourseId,
        row.CourseTitle,
        row.CourseCode,
        row.Category,
        row.Level,
        row.CoverImageUrl,
        row.LecturerName,
        row.Status,
        row.ProgressPercent,
        row.ModuleCount,
        row.LessonCount,
        row.TotalMinutes,
        row.FirstLessonTitle,
        row.EnrolledAtUtc,
        row.CompletedAtUtc);

    private static RecommendedCourseDto ToRecommendedDto(RecommendedCourseRow row) => new(
        row.CourseId,
        row.Title,
        row.Code,
        row.Category,
        row.Level,
        row.Price,
        row.CoverImageUrl,
        row.LecturerName,
        row.EnrolledCount,
        row.LessonCount);
}
