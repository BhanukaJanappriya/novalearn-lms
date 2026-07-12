using MediatR;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Courses.DeleteCourse;

/// <summary>Deletes a course. Admins may delete any; lecturers only their own.</summary>
public sealed record DeleteCourseCommand(Guid CourseId) : IRequest<Result>;
