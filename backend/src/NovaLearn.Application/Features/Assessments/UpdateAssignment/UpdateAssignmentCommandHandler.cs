using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Assessments.Common;
using NovaLearn.Domain.Assessments;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Assessments.UpdateAssignment;

public sealed class UpdateAssignmentCommandHandler(
    IAssessmentRepository assessments,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider dateTime)
    : IRequestHandler<UpdateAssignmentCommand, Result<AssignmentDto>>
{
    public async Task<Result<AssignmentDto>> Handle(
        UpdateAssignmentCommand request, CancellationToken cancellationToken)
    {
        Assignment? assignment = await assessments.GetAssignmentAsync(request.AssignmentId, cancellationToken);
        if (assignment is null)
        {
            return Result.Failure<AssignmentDto>(AssessmentErrors.AssignmentNotFound);
        }

        if (AssessmentAuthority.CheckCanManage(assignment.Course, currentUser) is { } denied)
        {
            return Result.Failure<AssignmentDto>(denied);
        }

        // Existing submissions keep the IsLate flag captured when they were handed in, so
        // moving the due date never rewrites history.
        assignment.Update(
            request.Title,
            request.Instructions,
            request.DueAtUtc,
            request.MaxPoints,
            request.AllowLateSubmissions,
            request.Status);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AssignmentDto.FromEntity(assignment, dateTime.UtcNow);
    }
}
