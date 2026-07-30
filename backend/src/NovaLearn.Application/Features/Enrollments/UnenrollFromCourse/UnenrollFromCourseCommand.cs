using MediatR;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Enrollments.UnenrollFromCourse;

/// <summary>Drops an enrolment. Students may drop their own; admins may drop any.</summary>
public sealed record UnenrollFromCourseCommand(Guid EnrollmentId) : IRequest<Result>;
