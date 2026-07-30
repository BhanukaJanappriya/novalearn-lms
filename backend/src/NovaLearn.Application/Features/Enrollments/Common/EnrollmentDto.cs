using NovaLearn.Domain.Enrollments;

namespace NovaLearn.Application.Features.Enrollments.Common;

/// <summary>
/// Read model for an enrolment. Carries both the student and the course facets so the same
/// shape serves "my courses" (course-centric) and the course roster (student-centric).
/// Enums are surfaced as their string names for the client.
/// </summary>
public sealed record EnrollmentDto(
    Guid Id,
    Guid StudentId,
    string StudentName,
    string StudentEmail,
    Guid CourseId,
    string CourseTitle,
    string CourseCode,
    string CourseCategory,
    string CourseLevel,
    string? CourseCoverImageUrl,
    string Status,
    int ProgressPercent,
    DateTimeOffset EnrolledAtUtc,
    DateTimeOffset? CompletedAtUtc)
{
    public static EnrollmentDto FromEntity(Enrollment enrollment) => new(
        enrollment.Id,
        enrollment.StudentId,
        enrollment.Student?.FullName ?? "Unknown",
        enrollment.Student?.Email ?? string.Empty,
        enrollment.CourseId,
        enrollment.Course?.Title ?? "Unknown course",
        enrollment.Course?.Code ?? string.Empty,
        enrollment.Course?.Category ?? string.Empty,
        enrollment.Course?.Level.ToString() ?? string.Empty,
        enrollment.Course?.CoverImageUrl,
        enrollment.Status.ToString(),
        enrollment.ProgressPercent,
        enrollment.EnrolledAtUtc,
        enrollment.CompletedAtUtc);
}
