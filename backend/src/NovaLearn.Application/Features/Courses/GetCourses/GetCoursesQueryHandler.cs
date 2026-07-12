using MediatR;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Courses.Common;
using NovaLearn.Domain.Courses;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Courses.GetCourses;

public sealed class GetCoursesQueryHandler(ICourseRepository courses)
    : IRequestHandler<GetCoursesQuery, Result<IReadOnlyList<CourseDto>>>
{
    public async Task<Result<IReadOnlyList<CourseDto>>> Handle(
        GetCoursesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Course> all = await courses.ListAsync(cancellationToken);
        IReadOnlyList<CourseDto> dtos = all.Select(CourseDto.FromEntity).ToList();
        return Result.Success(dtos);
    }
}
