using MediatR;
using NovaLearn.Application.Features.Assessments.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Assessments.GetAssignmentSubmissions;

/// <summary>Every submission for an assignment. Restricted to the owning lecturer or an admin.</summary>
public sealed record GetAssignmentSubmissionsQuery(Guid AssignmentId)
    : IRequest<Result<IReadOnlyList<SubmissionDto>>>;
