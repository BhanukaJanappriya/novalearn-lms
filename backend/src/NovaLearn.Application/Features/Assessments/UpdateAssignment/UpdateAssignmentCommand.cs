using MediatR;
using NovaLearn.Application.Features.Assessments.Common;
using NovaLearn.Domain.Assessments;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Assessments.UpdateAssignment;

public sealed record UpdateAssignmentCommand(
    Guid AssignmentId,
    string Title,
    string? Instructions,
    DateTimeOffset? DueAtUtc,
    int MaxPoints,
    bool AllowLateSubmissions,
    AssessmentStatus Status) : IRequest<Result<AssignmentDto>>;
