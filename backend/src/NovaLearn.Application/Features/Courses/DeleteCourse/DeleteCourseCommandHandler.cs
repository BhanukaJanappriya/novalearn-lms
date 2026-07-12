using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Courses.DeleteCourse;

public sealed class DeleteCourseCommandHandler(
    ICourseRepository courses,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteCourseCommand, Result>
{
    public async Task<Result> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
    {
        Course? course = await courses.GetByIdAsync(request.CourseId, cancellationToken);
        if (course is null)
        {
            return Result.Failure(CourseErrors.NotFound);
        }

        bool isAdmin =
            currentUser.IsInRole(Roles.Administrator) || currentUser.IsInRole(Roles.SuperAdministrator);

        // Lecturers may only delete their own courses; admins may delete any.
        if (!isAdmin && course.LecturerId != currentUser.UserId)
        {
            return Result.Failure(CourseErrors.NotOwner);
        }

        courses.Remove(course);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
