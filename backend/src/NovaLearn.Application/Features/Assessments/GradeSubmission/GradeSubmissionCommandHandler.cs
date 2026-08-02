using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Assessments.Common;
using NovaLearn.Domain.Assessments;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Assessments.GradeSubmission;

public sealed class GradeSubmissionCommandHandler(
    IAssessmentRepository assessments,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider dateTime)
    : IRequestHandler<GradeSubmissionCommand, Result<SubmissionDto>>
{
    public async Task<Result<SubmissionDto>> Handle(
        GradeSubmissionCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } graderId)
        {
            return Result.Failure<SubmissionDto>(AssessmentErrors.Unauthenticated);
        }

        Submission? submission = await assessments.GetSubmissionAsync(request.SubmissionId, cancellationToken);
        if (submission is null)
        {
            return Result.Failure<SubmissionDto>(AssessmentErrors.SubmissionNotFound);
        }

        if (AssessmentAuthority.CheckCanManage(submission.Assignment?.Course, currentUser) is { } denied)
        {
            return Result.Failure<SubmissionDto>(denied);
        }

        // The aggregate clamps to the assignment's ceiling, so a grader cannot award more than
        // the work is worth even if the request says otherwise.
        submission.Grade(
            request.PointsAwarded,
            request.Feedback,
            submission.Assignment!.MaxPoints,
            graderId,
            dateTime.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return SubmissionDto.FromEntity(submission);
    }
}
