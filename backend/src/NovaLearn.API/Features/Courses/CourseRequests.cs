using NovaLearn.Domain.Courses;

namespace NovaLearn.API.Features.Courses;

/// <summary>Body for creating a course. Enums accept their string names (e.g. "Beginner").</summary>
public sealed record CreateCourseRequest(
    string Title,
    string Code,
    string? Description,
    string Category,
    CourseLevel Level,
    CourseStatus Status,
    decimal Price,
    string? CoverImageUrl);
