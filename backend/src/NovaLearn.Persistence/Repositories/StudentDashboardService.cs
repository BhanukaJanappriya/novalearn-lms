using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Enrollments;

namespace NovaLearn.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the student dashboard read model. Soft-delete query filters apply
/// automatically, so dropped-and-deleted enrolments and deleted lessons never leak in.
/// </summary>
internal sealed class StudentDashboardService(ApplicationDbContext context) : IStudentDashboardService
{
    /// <summary>How many unjoined courses to suggest.</summary>
    private const int RecommendationCount = 4;

    public async Task<StudentStatistics> GetForStudentAsync(
        Guid studentId, CancellationToken cancellationToken)
    {
        List<StudentEnrollmentRow> enrollments = await GetEnrollmentsAsync(studentId, cancellationToken);
        List<RecommendedCourseRow> recommended = await GetRecommendedAsync(studentId, cancellationToken);

        // Bucket in memory to avoid provider-specific date-part grouping, matching the approach
        // AdminStatisticsService already takes for registrations.
        List<MonthlyCount> monthly = enrollments
            .GroupBy(e => new { e.EnrolledAtUtc.Year, e.EnrolledAtUtc.Month })
            .Select(g => new MonthlyCount(g.Key.Year, g.Key.Month, g.Count()))
            .ToList();

        return new StudentStatistics(enrollments, recommended, monthly);
    }

    private async Task<List<StudentEnrollmentRow>> GetEnrollmentsAsync(
        Guid studentId, CancellationToken cancellationToken)
    {
        var rows = await context.Enrollments
            .Where(e => e.StudentId == studentId && e.Status != EnrollmentStatus.Dropped)
            .OrderByDescending(e => e.EnrolledAtUtc)
            .Select(e => new
            {
                EnrollmentId = e.Id,
                e.CourseId,
                CourseTitle = e.Course!.Title,
                CourseCode = e.Course.Code,
                e.Course.Category,
                e.Course.Level,
                e.Course.CoverImageUrl,
                LecturerFirstName = e.Course.Lecturer == null ? null : e.Course.Lecturer.FirstName,
                LecturerLastName = e.Course.Lecturer == null ? null : e.Course.Lecturer.LastName,
                e.Status,
                e.ProgressPercent,
                e.EnrolledAtUtc,
                e.CompletedAtUtc,
                ModuleCount = context.CourseModules.Count(m => m.CourseId == e.CourseId),
                LessonCount = context.Lessons.Count(l => l.Module!.CourseId == e.CourseId),
                TotalMinutes = context.Lessons
                    .Where(l => l.Module!.CourseId == e.CourseId)
                    .Sum(l => (int?)l.DurationMinutes) ?? 0,

                // The opening lesson of the opening module, used as the "starts with" hint.
                FirstLessonTitle = context.Lessons
                    .Where(l => l.Module!.CourseId == e.CourseId)
                    .OrderBy(l => l.Module!.SortOrder)
                    .ThenBy(l => l.SortOrder)
                    .Select(l => l.Title)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        // Enum-to-string mapping happens in memory so no provider translation is needed.
        return rows
            .Select(r => new StudentEnrollmentRow(
                r.EnrollmentId,
                r.CourseId,
                r.CourseTitle,
                r.CourseCode,
                r.Category,
                r.Level.ToString(),
                r.CoverImageUrl,
                FullName(r.LecturerFirstName, r.LecturerLastName),
                r.Status.ToString(),
                r.ProgressPercent,
                r.EnrolledAtUtc,
                r.CompletedAtUtc,
                r.ModuleCount,
                r.LessonCount,
                r.TotalMinutes,
                r.FirstLessonTitle))
            .ToList();
    }

    private async Task<List<RecommendedCourseRow>> GetRecommendedAsync(
        Guid studentId, CancellationToken cancellationToken)
    {
        var rows = await context.Courses
            .Where(c => c.Status == CourseStatus.Published)
            .Where(c => !context.Enrollments.Any(e =>
                e.CourseId == c.Id && e.StudentId == studentId && e.Status != EnrollmentStatus.Dropped))
            .Select(c => new
            {
                CourseId = c.Id,
                c.Title,
                c.Code,
                c.Category,
                c.Level,
                c.Price,
                c.CoverImageUrl,
                LecturerFirstName = c.Lecturer == null ? null : c.Lecturer.FirstName,
                LecturerLastName = c.Lecturer == null ? null : c.Lecturer.LastName,
                EnrolledCount = context.Enrollments
                    .Count(e => e.CourseId == c.Id && e.Status != EnrollmentStatus.Dropped),
                LessonCount = context.Lessons.Count(l => l.Module!.CourseId == c.Id)
            })
            .OrderByDescending(c => c.EnrolledCount)
            .ThenByDescending(c => c.LessonCount)
            .Take(RecommendationCount)
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new RecommendedCourseRow(
                r.CourseId,
                r.Title,
                r.Code,
                r.Category,
                r.Level.ToString(),
                r.Price,
                r.CoverImageUrl,
                FullName(r.LecturerFirstName, r.LecturerLastName),
                r.EnrolledCount,
                r.LessonCount))
            .ToList();
    }

    private static string FullName(string? first, string? last) =>
        $"{first} {last}".Trim() is { Length: > 0 } name ? name : "Unknown";
}
