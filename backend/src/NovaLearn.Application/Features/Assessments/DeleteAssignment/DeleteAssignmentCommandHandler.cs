using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Assessments.Common;
using NovaLearn.Domain.Assessments;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Assessments.DeleteAssignment;

public sealed class DeleteAssignmentCommandHandler(
    IAssessmentRepository assessments,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteAssignmentCommand, Result>
{
    public async Task<Result> Handle(DeleteAssignmentCommand request, CancellationToken cancellationToken)
    {
        Assignment? assignment = await assessments.GetAssignmentAsync(request.AssignmentId, cancellationToken);
        if (assignment is null)
        {
            return Result.Failure(AssessmentErrors.AssignmentNotFound);
        }

        if (AssessmentAuthority.CheckCanManage(assignment.Course, currentUser) is { } denied)
        {
            return Result.Failure(denied);
        }

        // Soft delete only. Submissions keep their rows, and the partial unique index ignores
        // deleted ones, so nothing is stranded and a learner could submit again if it returned.
        assessments.RemoveAssignment(assignment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
