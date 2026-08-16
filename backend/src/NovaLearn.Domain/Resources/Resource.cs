using NovaLearn.Domain.Common;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Domain.Resources;

/// <summary>
/// A piece of material posted to the platform wall: uploaded notes, a video, or a link to
/// something hosted elsewhere.
///
/// A resource is either uploaded or external, never both. That is enforced here rather than left
/// to the callers, because the two carry different fields and a row holding both would leave every
/// reader guessing which one to render.
/// </summary>
public sealed class Resource : BaseEntity
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    private Resource() { } // EF Core

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public ResourceKind Kind { get; private set; }

    /// <summary>Where an external resource lives. Null for uploads.</summary>
    public string? Url { get; private set; }

    /// <summary>
    /// The opaque name the file was stored under. Never the name the uploader chose: that is kept
    /// separately for display, so nothing a user typed is ever used as a path.
    /// </summary>
    public string? StoredFileKey { get; private set; }

    /// <summary>The name the file had when it was uploaded, shown and used for downloads.</summary>
    public string? OriginalFileName { get; private set; }

    /// <summary>The type the file will be served as, resolved from its extension on upload.</summary>
    public string? ContentType { get; private set; }

    public long? SizeBytes { get; private set; }

    /// <summary>Set only for <see cref="ResourceKind.YouTube"/>, and the basis of the thumbnail.</summary>
    public string? YouTubeVideoId { get; private set; }

    /// <summary>
    /// The course this belongs to, or null when it is for the whole platform. This is what decides
    /// who sees the post, so it is also an access control field and not merely a label.
    /// </summary>
    public Guid? CourseId { get; private set; }

    public Guid PostedById { get; private set; }

    public Course? Course { get; private set; }

    public ApplicationUser? PostedBy { get; private set; }

    /// <summary>Whether this resource's bytes live on our own storage.</summary>
    public bool IsUpload => StoredFileKey is not null;

    /// <summary>The still image for a YouTube post, and null for everything else.</summary>
    public string? ThumbnailUrl => ResourceAddress.YouTubeThumbnailUrl(YouTubeVideoId);

    /// <summary>
    /// Posts a link. The kind is classified from the address, so YouTube and Drive are recognised
    /// without the poster having to say which is which.
    /// </summary>
    public static Resource ForLink(
        string title, string? description, string url, Guid? courseId, Guid postedById)
    {
        if (!ResourceAddress.IsUsable(url))
        {
            throw new ArgumentException("A link must be an absolute http or https address.", nameof(url));
        }

        (ResourceKind kind, string? videoId) = ResourceAddress.Classify(url);

        return new Resource
        {
            Title = Clean(title, TitleMaxLength)
                ?? throw new ArgumentException("A resource needs a title.", nameof(title)),
            Description = Clean(description, DescriptionMaxLength),
            Kind = kind,
            Url = url.Trim(),
            YouTubeVideoId = videoId,
            CourseId = courseId,
            PostedById = postedById
        };
    }

    /// <summary>
    /// Records a file that has already been written to storage. The kind and content type are
    /// resolved from the extension by the caller, never taken from the browser.
    /// </summary>
    public static Resource ForUpload(
        string title,
        string? description,
        ResourceKind kind,
        string storedFileKey,
        string originalFileName,
        string contentType,
        long sizeBytes,
        Guid? courseId,
        Guid postedById) =>
        new()
        {
            Title = Clean(title, TitleMaxLength)
                ?? throw new ArgumentException("A resource needs a title.", nameof(title)),
            Description = Clean(description, DescriptionMaxLength),
            Kind = kind,
            StoredFileKey = storedFileKey,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            SizeBytes = sizeBytes < 0 ? 0 : sizeBytes,
            CourseId = courseId,
            PostedById = postedById
        };

    /// <summary>
    /// Edits the parts a poster is allowed to change after the fact.
    ///
    /// What the resource points at is not among them. Swapping the address or the file underneath
    /// a post would let an innocuous looking item become something else after people have seen it,
    /// so changing the material means posting it again.
    /// </summary>
    public void Describe(string title, string? description, Guid? courseId)
    {
        Title = Clean(title, TitleMaxLength)
            ?? throw new ArgumentException("A resource needs a title.", nameof(title));
        Description = Clean(description, DescriptionMaxLength);
        CourseId = courseId;
    }

    private static string? Clean(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
