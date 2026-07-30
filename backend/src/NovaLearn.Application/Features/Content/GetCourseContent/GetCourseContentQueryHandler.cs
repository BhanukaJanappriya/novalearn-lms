using MediatR;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Content.Common;
using NovaLearn.Domain.Content;
using NovaLearn.Domain.Courses;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Content.GetCourseContent;

public sealed class GetCourseContentQueryHandler(
    ICourseRepository courses,
    ICourseContentRepository content,
    ICurrentUser currentUser)
    : IRequestHandler<GetCourseContentQuery, Result<CourseContentDto>>
{
    public async Task<Result<CourseContentDto>> Handle(
        GetCourseContentQuery request, CancellationToken cancellationToken)
    {
        Course? course = await courses.GetByIdAsync(request.CourseId, cancellationToken);

        if (ContentAuthority.CheckCanRead(course, currentUser) is Error error)
        {
            return Result.Failure<CourseContentDto>(error);
        }

        IReadOnlyList<CourseModule> modules =
            await content.GetModulesForCourseAsync(request.CourseId, cancellationToken);

        return CourseContentDto.FromEntities(course!, modules);
    }
}
