using MediatR;
using NovaLearn.Application.Features.Enrollments.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Enrollments.GetMyEnrollments;

/// <summary>Lists the calling user's enrolments, newest first.</summary>
public sealed record GetMyEnrollmentsQuery : IRequest<Result<IReadOnlyList<EnrollmentDto>>>;
