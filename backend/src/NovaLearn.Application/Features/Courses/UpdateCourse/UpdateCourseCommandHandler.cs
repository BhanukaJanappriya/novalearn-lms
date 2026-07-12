using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Courses.Common;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Courses.UpdateCourse;

public sealed class UpdateCourseCommandHandler(
    ICourseRepository courses,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<UpdateCourseCommand, Result<CourseDto>>
{
    public async Task<Result<CourseDto>> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        Course? course = await courses.GetByIdAsync(request.CourseId, cancellationToken);
        if (course is null)
        {
            return Result.Failure<CourseDto>(CourseErrors.NotFound);
        }

        bool isAdmin =
            currentUser.IsInRole(Roles.Administrator) || currentUser.IsInRole(Roles.SuperAdministrator);

        // Lecturers may only edit their own courses; admins may edit any.
        if (!isAdmin && course.LecturerId != currentUser.UserId)
        {
            return Result.Failure<CourseDto>(CourseErrors.NotOwner);
        }

        // If the code is changing, make sure the new one is not taken by another course.
        string normalisedCode = request.Code.Trim().ToUpperInvariant();
        if (normalisedCode != course.Code && await courses.CodeExistsAsync(normalisedCode, cancellationToken))
        {
            return Result.Failure<CourseDto>(CourseErrors.DuplicateCode);
        }

        course.Update(
            request.Title,
            request.Code,
            request.Description,
            request.Category,
            request.Level,
            request.Status,
            request.Price,
            request.CoverImageUrl);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CourseDto.FromEntity(course);
    }
}
