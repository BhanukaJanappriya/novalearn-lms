using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Enrollments.Common;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Enrollments.GetCourseRoster;

public sealed class GetCourseRosterQueryHandler(
    ICourseRepository courses,
    IEnrollmentRepository enrollments,
    ICurrentUser currentUser)
    : IRequestHandler<GetCourseRosterQuery, Result<IReadOnlyList<EnrollmentDto>>>
{
    public async Task<Result<IReadOnlyList<EnrollmentDto>>> Handle(
        GetCourseRosterQuery request, CancellationToken cancellationToken)
    {
        Course? course = await courses.GetByIdAsync(request.CourseId, cancellationToken);
        if (course is null)
        {
            return Result.Failure<IReadOnlyList<EnrollmentDto>>(CourseErrors.NotFound);
        }

        bool isAdmin =
            currentUser.IsInRole(Roles.Administrator) || currentUser.IsInRole(Roles.SuperAdministrator);

        // Lecturers may only see the roster of their own courses; admins may see any.
        if (!isAdmin && course.LecturerId != currentUser.UserId)
        {
            return Result.Failure<IReadOnlyList<EnrollmentDto>>(EnrollmentErrors.NotCourseOwner);
        }

        IReadOnlyList<Enrollment> roster = await enrollments.ListForCourseAsync(course.Id, cancellationToken);
        IReadOnlyList<EnrollmentDto> dtos = roster.Select(EnrollmentDto.FromEntity).ToList();

        return Result.Success(dtos);
    }
}
