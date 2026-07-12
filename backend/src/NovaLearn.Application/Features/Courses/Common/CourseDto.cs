using NovaLearn.Domain.Courses;

namespace NovaLearn.Application.Features.Courses.Common;

/// <summary>Read model for a course. Enums are surfaced as their string names for the client.</summary>
public sealed record CourseDto(
    Guid Id,
    string Title,
    string Code,
    string? Description,
    string Category,
    string Level,
    string Status,
    decimal Price,
    string? CoverImageUrl,
    Guid LecturerId,
    string LecturerName,
    DateTimeOffset CreatedAtUtc)
{
    public static CourseDto FromEntity(Course course) => new(
        course.Id,
        course.Title,
        course.Code,
        course.Description,
        course.Category,
        course.Level.ToString(),
        course.Status.ToString(),
        course.Price,
        course.CoverImageUrl,
        course.LecturerId,
        course.Lecturer?.FullName ?? "Unknown",
        course.CreatedAtUtc);
}
