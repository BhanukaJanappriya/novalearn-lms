using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Enrollments.UnenrollFromCourse;

public sealed class UnenrollFromCourseCommandHandler(
    IEnrollmentRepository enrollments,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<UnenrollFromCourseCommand, Result>
{
    public async Task<Result> Handle(UnenrollFromCourseCommand request, CancellationToken cancellationToken)
    {
        Enrollment? enrollment = await enrollments.GetByIdAsync(request.EnrollmentId, cancellationToken);
        if (enrollment is null)
        {
            return Result.Failure(EnrollmentErrors.NotFound);
        }

        bool isAdmin =
            currentUser.IsInRole(Roles.Administrator) || currentUser.IsInRole(Roles.SuperAdministrator);

        // Students may only drop their own enrolments; admins may drop any.
        if (!isAdmin && enrollment.StudentId != currentUser.UserId)
        {
            return Result.Failure(EnrollmentErrors.NotOwner);
        }

        // Record the withdrawal on the aggregate, then soft-delete the row. The partial unique
        // index ignores deleted rows, so the student can enrol again later.
        enrollment.Drop();
        enrollments.Remove(enrollment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
