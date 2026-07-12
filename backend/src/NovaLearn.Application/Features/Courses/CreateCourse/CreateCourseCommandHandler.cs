using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Courses.Common;
using NovaLearn.Domain.Courses;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Courses.CreateCourse;

public sealed class CreateCourseCommandHandler(
    ICourseRepository courses,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<CreateCourseCommand, Result<CourseDto>>
{
    public async Task<Result<CourseDto>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid lecturerId)
        {
            return Result.Failure<CourseDto>(CourseErrors.Unauthenticated);
        }

        string normalisedCode = request.Code.Trim().ToUpperInvariant();
        if (await courses.CodeExistsAsync(normalisedCode, cancellationToken))
        {
            return Result.Failure<CourseDto>(CourseErrors.DuplicateCode);
        }

        Course course = Course.Create(
            request.Title,
            request.Code,
            request.Description,
            request.Category,
            request.Level,
            request.Status,
            request.Price,
            request.CoverImageUrl,
            lecturerId);

        await courses.AddAsync(course, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with the lecturer navigation so the response carries the owner's name.
        Course created = await courses.GetByIdAsync(course.Id, cancellationToken) ?? course;
        return CourseDto.FromEntity(created);
    }
}
