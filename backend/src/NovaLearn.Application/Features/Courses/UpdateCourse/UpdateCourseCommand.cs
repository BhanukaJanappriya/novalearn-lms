using MediatR;
using NovaLearn.Application.Features.Courses.Common;
using NovaLearn.Domain.Courses;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Courses.UpdateCourse;

/// <summary>Edits a course. Admins may edit any; lecturers only their own.</summary>
public sealed record UpdateCourseCommand(
    Guid CourseId,
    string Title,
    string Code,
    string? Description,
    string Category,
    CourseLevel Level,
    CourseStatus Status,
    decimal Price,
    string? CoverImageUrl) : IRequest<Result<CourseDto>>;
