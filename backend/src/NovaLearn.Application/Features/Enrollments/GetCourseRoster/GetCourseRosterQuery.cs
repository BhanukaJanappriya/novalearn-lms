using MediatR;
using NovaLearn.Application.Features.Enrollments.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Enrollments.GetCourseRoster;

/// <summary>Lists the students enrolled in a course. Restricted to the owning lecturer or an admin.</summary>
public sealed record GetCourseRosterQuery(Guid CourseId) : IRequest<Result<IReadOnlyList<EnrollmentDto>>>;
