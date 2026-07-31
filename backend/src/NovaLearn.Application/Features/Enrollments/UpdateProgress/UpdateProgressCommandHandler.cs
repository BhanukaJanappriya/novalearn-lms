using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Enrollments.Common;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Enrollments.UpdateProgress;

public sealed class UpdateProgressCommandHandler(
    IEnrollmentRepository enrollments,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider dateTime)
    : IRequestHandler<UpdateProgressCommand, Result<EnrollmentDto>>
{
    public async Task<Result<EnrollmentDto>> Handle(
        UpdateProgressCommand request, CancellationToken cancellationToken)
    {
        Enrollment? enrollment = await enrollments.GetByIdAsync(request.EnrollmentId, cancellationToken);
        if (enrollment is null)
        {
            return Result.Failure<EnrollmentDto>(EnrollmentErrors.NotFound);
        }

        bool isAdmin =
            currentUser.IsInRole(Roles.Administrator) || currentUser.IsInRole(Roles.SuperAdministrator);

        // Learners record their own progress; admins may correct anyone's.
        if (!isAdmin && enrollment.StudentId != currentUser.UserId)
        {
            return Result.Failure<EnrollmentDto>(EnrollmentErrors.NotOwner);
        }

        // A dropped enrolment has to be rejoined before progress means anything again.
        if (enrollment.Status == EnrollmentStatus.Dropped)
        {
            return Result.Failure<EnrollmentDto>(EnrollmentErrors.NotActive);
        }

        // The aggregate owns the status/completion transitions that follow from the new figure.
        enrollment.UpdateProgress(request.ProgressPercent, dateTime.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return EnrollmentDto.FromEntity(enrollment);
    }
}
