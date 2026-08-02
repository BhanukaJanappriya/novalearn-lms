using MediatR;
using NovaLearn.Application.Features.Assessments.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Assessments.SubmitAssignment;

/// <summary>
/// Hands work in. Submitting again replaces the previous attempt, so this is idempotent from
/// the learner's point of view: there is only ever one live submission.
/// </summary>
public sealed record SubmitAssignmentCommand(Guid AssignmentId, string Content, string? AttachmentUrl)
    : IRequest<Result<SubmissionDto>>;
