using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Resources.Common;
using NovaLearn.Domain.Resources;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Resources.GetWall;

/// <summary>The platform wall, newest first, optionally narrowed by course, kind or search.</summary>
public sealed record GetWallQuery(Guid? CourseId, ResourceKind? Kind, string? Search)
    : IRequest<Result<IReadOnlyList<ResourceDto>>>;

/// <summary>
/// Builds the wall for whoever is asking.
///
/// Visibility is a filter rather than a check: the set of courses the caller may see is worked out
/// first and the query is built against it, so a post from a course they are not on never enters
/// the result. Posts attached to no course are platform wide and always included.
/// </summary>
public sealed class GetWallQueryHandler(
    IResourceRepository resources, ICurrentUser currentUser)
    : IRequestHandler<GetWallQuery, Result<IReadOnlyList<ResourceDto>>>
{
    public async Task<Result<IReadOnlyList<ResourceDto>>> Handle(
        GetWallQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId)
        {
            return Result.Failure<IReadOnlyList<ResourceDto>>(ResourceErrors.Unauthenticated);
        }

        bool seesEverything = ResourceAuthority.IsAdmin(currentUser);

        IReadOnlyList<Guid> visibleCourseIds = seesEverything
            ? []
            : await resources.VisibleCourseIdsAsync(callerId, cancellationToken);

        IReadOnlyList<Resource> wall = await resources.ListWallAsync(
            visibleCourseIds,
            seesEverything,
            request.CourseId,
            request.Kind,
            request.Search,
            cancellationToken);

        return Result.Success<IReadOnlyList<ResourceDto>>(
            wall
                .Select(resource =>
                    ResourceMapper.ToDto(resource, ResourceAuthority.CanManage(resource, currentUser)))
                .ToList());
    }
}
