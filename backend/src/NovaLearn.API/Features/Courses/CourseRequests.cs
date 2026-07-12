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

/// <summary>Body for editing a course (the id comes from the route).</summary>
public sealed record UpdateCourseRequest(
    string Title,
    string Code,
    string? Description,
    string Category,
    CourseLevel Level,
    CourseStatus Status,
    decimal Price,
    string? CoverImageUrl);
