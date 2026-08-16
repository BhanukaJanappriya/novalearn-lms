using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Resources.Common;
using NovaLearn.Domain.Resources;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Resources.DownloadFile;

/// <summary>An uploaded file, ready to be written to the response.</summary>
/// <param name="Inline">
/// Whether the browser may render this in a tab. False means it is served as a download, which is
/// how anything the browser might interpret is kept from executing in our origin.
/// </param>
public sealed record ResourceFile(
    Stream Content, string ContentType, string FileName, bool Inline);

/// <summary>Fetches the bytes behind an uploaded resource, if the caller may see it.</summary>
public sealed record GetResourceFileQuery(Guid ResourceId) : IRequest<Result<ResourceFile>>;

/// <summary>
/// Serves an upload.
///
/// The visibility rule is applied here rather than being left to the storage layer, which knows
/// nothing about courses. Stored keys are never exposed, so this endpoint is the only way to the
/// bytes and therefore the only place the rule has to hold.
/// </summary>
public sealed class GetResourceFileQueryHandler(
    IResourceRepository resources, IFileStorage storage, ICurrentUser currentUser)
    : IRequestHandler<GetResourceFileQuery, Result<ResourceFile>>
{
    public async Task<Result<ResourceFile>> Handle(
        GetResourceFileQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId)
        {
            return Result.Failure<ResourceFile>(ResourceErrors.Unauthenticated);
        }

        Resource? resource = await resources.GetByIdAsync(request.ResourceId, cancellationToken);

        if (resource is null || resource.StoredFileKey is null)
        {
            return Result.Failure<ResourceFile>(ResourceErrors.NotFound);
        }

        if (!await CanSeeAsync(resource, callerId, cancellationToken))
        {
            return Result.Failure<ResourceFile>(ResourceErrors.NotVisible);
        }

        Stream? content = await storage.OpenReadAsync(resource.StoredFileKey, cancellationToken);

        if (content is null)
        {
            return Result.Failure<ResourceFile>(ResourceErrors.FileNotFound);
        }

        return Result.Success(new ResourceFile(
            content,
            resource.ContentType ?? "application/octet-stream",
            resource.OriginalFileName ?? "download",
            UploadedFileTypes.IsSafeToDisplayInline(resource.Kind)));
    }

    private async Task<bool> CanSeeAsync(
        Resource resource, Guid callerId, CancellationToken cancellationToken)
    {
        // Platform wide posts are open to anyone signed in.
        if (resource.CourseId is not { } courseId)
        {
            return true;
        }

        if (ResourceAuthority.IsAdmin(currentUser) || resource.PostedById == callerId)
        {
            return true;
        }

        IReadOnlyList<Guid> visible = await resources.VisibleCourseIdsAsync(callerId, cancellationToken);
        return visible.Contains(courseId);
    }
}
