using MediatR;
using NovaLearn.Application.Features.Assessments.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Assessments.GradeSubmission;

public sealed record GradeSubmissionCommand(Guid SubmissionId, int PointsAwarded, string? Feedback)
    : IRequest<Result<SubmissionDto>>;
