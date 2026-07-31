namespace NovaLearn.Application.Common.Models;

/// <summary>
/// Real, database-backed facts for one learner's dashboard. Every figure here comes from live
/// tables (enrolments, courses, modules, lessons); the query handler only shapes and orders
/// them. Nothing on the student dashboard is synthesised.
/// </summary>
public sealed record StudentStatistics(
    IReadOnlyList<StudentEnrollmentRow> Enrollments,
    IReadOnlyList<RecommendedCourseRow> Recommended,
    IReadOnlyList<MonthlyCount> MonthlyEnrollments);

/// <summary>
/// One of the learner's non-dropped enrolments, joined to its course and to that course's
/// content totals so the dashboard can show scale without a second round trip.
/// </summary>
public sealed record StudentEnrollmentRow(
    Guid EnrollmentId,
    Guid CourseId,
    string CourseTitle,
    string CourseCode,
    string Category,
    string Level,
    string? CoverImageUrl,
    string LecturerName,
    string Status,
    int ProgressPercent,
    DateTimeOffset EnrolledAtUtc,
    DateTimeOffset? CompletedAtUtc,
    int ModuleCount,
    int LessonCount,
    int TotalMinutes,
    string? FirstLessonTitle);

/// <summary>A published course the learner has not joined, ranked by how popular it is.</summary>
public sealed record RecommendedCourseRow(
    Guid CourseId,
    string Title,
    string Code,
    string Category,
    string Level,
    decimal Price,
    string? CoverImageUrl,
    string LecturerName,
    int EnrolledCount,
    int LessonCount);
