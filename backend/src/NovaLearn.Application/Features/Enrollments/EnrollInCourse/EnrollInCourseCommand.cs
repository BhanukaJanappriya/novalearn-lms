using MediatR;
using NovaLearn.Application.Features.Enrollments.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Enrollments.EnrollInCourse;

/// <summary>Enrols the calling student in a published course.</summary>
public sealed record EnrollInCourseCommand(Guid CourseId) : IRequest<Result<EnrollmentDto>>;
