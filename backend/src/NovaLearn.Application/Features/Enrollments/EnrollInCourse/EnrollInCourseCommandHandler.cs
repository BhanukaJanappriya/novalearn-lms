using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Enrollments.Common;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Enrollments.EnrollInCourse;

public sealed class EnrollInCourseCommandHandler(
    ICourseRepository courses,
    IEnrollmentRepository enrollments,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<EnrollInCourseCommand, Result<EnrollmentDto>>
{
    public async Task<Result<EnrollmentDto>> Handle(
        EnrollInCourseCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid studentId)
        {
            return Result.Failure<EnrollmentDto>(EnrollmentErrors.Unauthenticated);
        }

        Course? course = await courses.GetByIdAsync(request.CourseId, cancellationToken);
        if (course is null)
        {
            return Result.Failure<EnrollmentDto>(CourseErrors.NotFound);
        }

        // Draft courses are still being authored, so they are not open to learners.
        if (course.Status != CourseStatus.Published)
        {
            return Result.Failure<EnrollmentDto>(EnrollmentErrors.CourseNotPublished);
        }

        Enrollment? existing = await enrollments.GetActiveAsync(studentId, course.Id, cancellationToken);
        if (existing is not null)
        {
            return Result.Failure<EnrollmentDto>(EnrollmentErrors.AlreadyEnrolled);
        }

        Enrollment enrollment = Enrollment.Create(studentId, course.Id, dateTimeProvider.UtcNow);

        await enrollments.AddAsync(enrollment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with the student/course navigations so the response is fully populated.
        Enrollment created = await enrollments.GetByIdAsync(enrollment.Id, cancellationToken) ?? enrollment;
        return EnrollmentDto.FromEntity(created);
    }
}
