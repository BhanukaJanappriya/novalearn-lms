using MediatR;
using NovaLearn.Application.Features.Courses.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Courses.GetCourses;

/// <summary>Lists all courses (newest first) for management.</summary>
public sealed record GetCoursesQuery : IRequest<Result<IReadOnlyList<CourseDto>>>;
