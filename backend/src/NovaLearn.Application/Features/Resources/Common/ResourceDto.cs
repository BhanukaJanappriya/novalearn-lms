using NovaLearn.Domain.Resources;

namespace NovaLearn.Application.Features.Resources.Common;

/// <summary>
/// A wall post as the client sees it.
///
/// Uploads are addressed by <see cref="FileUrl"/>, an endpoint on this API rather than a path into
/// storage, so where the bytes actually live never reaches the browser and cannot be requested
/// without going through the visibility check.
/// </summary>
public sealed record ResourceDto(
    Guid Id,
    string Title,
    string? Description,
    ResourceKind Kind,
    string? Url,
    string? FileUrl,
    string? OriginalFileName,
    long? SizeBytes,
    string? ThumbnailUrl,
    string? EmbedUrl,
    Guid? CourseId,
    string? CourseTitle,
    Guid PostedById,
    string PostedByName,
    string? PostedByAvatarUrl,
    DateTimeOffset PostedAtUtc,
    bool CanManage);

/// <summary>Maps the aggregate onto the wire shape.</summary>
public static class ResourceMapper
{
    public static ResourceDto ToDto(Resource resource, bool canManage) =>
        new(
            resource.Id,
            resource.Title,
            resource.Description,
            resource.Kind,
            resource.Url,
            resource.IsUpload ? $"/api/v1/resources/{resource.Id}/file" : null,
            resource.OriginalFileName,
            resource.SizeBytes,
            resource.ThumbnailUrl,
            ResourceAddress.YouTubeEmbedUrl(resource.YouTubeVideoId),
            resource.CourseId,
            resource.Course?.Title,
            resource.PostedById,
            resource.PostedBy is { } poster
                ? $"{poster.FirstName} {poster.LastName}".Trim()
                : "Unknown",
            resource.PostedBy?.AvatarUrl,
            resource.CreatedAtUtc,
            canManage);
}
