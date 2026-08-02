using MediatR;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Assessments.DeleteAssignment;

public sealed record DeleteAssignmentCommand(Guid AssignmentId) : IRequest<Result>;
