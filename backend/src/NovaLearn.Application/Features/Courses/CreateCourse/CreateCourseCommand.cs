using MediatR;
using NovaLearn.Application.Features.Courses.Common;
using NovaLearn.Domain.Courses;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Courses.CreateCourse;

/// <summary>Creates a course owned by the current user (a lecturer or admin).</summary>
public sealed record CreateCourseCommand(
    string Title,
    string Code,
    string? Description,
    string Category,
    CourseLevel Level,
    CourseStatus Status,
    decimal Price,
    string? CoverImageUrl) : IRequest<Result<CourseDto>>;
