using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Assessments.Common;
using NovaLearn.Domain.Assessments;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Assessments.GetAssignmentSubmissions;

public sealed class GetAssignmentSubmissionsQueryHandler(
    IAssessmentRepository assessments,
    ICurrentUser currentUser)
    : IRequestHandler<GetAssignmentSubmissionsQuery, Result<IReadOnlyList<SubmissionDto>>>
{
    public async Task<Result<IReadOnlyList<SubmissionDto>>> Handle(
        GetAssignmentSubmissionsQuery request, CancellationToken cancellationToken)
    {
        Assignment? assignment = await assessments.GetAssignmentAsync(request.AssignmentId, cancellationToken);
        if (assignment is null)
        {
            return Result.Failure<IReadOnlyList<SubmissionDto>>(AssessmentErrors.AssignmentNotFound);
        }

        if (AssessmentAuthority.CheckCanManage(assignment.Course, currentUser) is { } denied)
        {
            return Result.Failure<IReadOnlyList<SubmissionDto>>(denied);
        }

        IReadOnlyList<Submission> submissions =
            await assessments.ListSubmissionsAsync(request.AssignmentId, cancellationToken);

        // The rows come back without their assignment navigation; the DTO needs the title and
        // points ceiling, and we already hold the one assignment they all belong to.
        return submissions
            .Select(s => SubmissionDto.FromEntity(s) with
            {
                AssignmentTitle = assignment.Title,
                MaxPoints = assignment.MaxPoints
            })
            .ToList();
    }
}
