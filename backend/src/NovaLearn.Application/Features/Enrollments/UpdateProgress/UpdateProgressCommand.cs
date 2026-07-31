using MediatR;
using NovaLearn.Application.Features.Enrollments.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Enrollments.UpdateProgress;

/// <summary>Records how far a learner has got through a course they are enrolled in.</summary>
public sealed record UpdateProgressCommand(Guid EnrollmentId, int ProgressPercent)
    : IRequest<Result<EnrollmentDto>>;
