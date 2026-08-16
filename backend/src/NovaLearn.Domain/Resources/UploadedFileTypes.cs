namespace NovaLearn.Domain.Resources;

/// <summary>
/// The file types the platform accepts, and what each one is.
///
/// This is an allowlist on purpose. Anything not named here is refused rather than stored and
/// worried about later, which keeps executables and scripts out of the upload directory entirely.
/// </summary>
public static class UploadedFileTypes
{
    private static readonly Dictionary<string, (ResourceKind Kind, string ContentType)> Accepted =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = (ResourceKind.Pdf, "application/pdf"),

            [".mp4"] = (ResourceKind.Video, "video/mp4"),
            [".webm"] = (ResourceKind.Video, "video/webm"),
            [".mov"] = (ResourceKind.Video, "video/quicktime"),
            [".m4v"] = (ResourceKind.Video, "video/x-m4v"),

            [".png"] = (ResourceKind.Image, "image/png"),
            [".jpg"] = (ResourceKind.Image, "image/jpeg"),
            [".jpeg"] = (ResourceKind.Image, "image/jpeg"),
            [".gif"] = (ResourceKind.Image, "image/gif"),
            [".webp"] = (ResourceKind.Image, "image/webp"),

            [".doc"] = (ResourceKind.Document, "application/msword"),
            [".docx"] = (ResourceKind.Document,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document"),
            [".ppt"] = (ResourceKind.Document, "application/vnd.ms-powerpoint"),
            [".pptx"] = (ResourceKind.Document,
                "application/vnd.openxmlformats-officedocument.presentationml.presentation"),
            [".xls"] = (ResourceKind.Document, "application/vnd.ms-excel"),
            [".xlsx"] = (ResourceKind.Document,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
            [".txt"] = (ResourceKind.Document, "text/plain"),
            [".md"] = (ResourceKind.Document, "text/markdown"),
            [".csv"] = (ResourceKind.Document, "text/csv"),
            [".zip"] = (ResourceKind.Document, "application/zip"),
        };

    /// <summary>Every accepted extension, for showing the learner what they may attach.</summary>
    public static IReadOnlyCollection<string> Extensions => Accepted.Keys;

    /// <summary>
    /// Resolves an extension to what it is and the type it will be served as.
    ///
    /// The content type comes from this table rather than from the upload, because the browser
    /// sends whatever it likes and we are the ones who have to serve the bytes back safely.
    /// </summary>
    public static bool TryResolve(string fileName, out ResourceKind kind, out string contentType)
    {
        kind = default;
        contentType = string.Empty;

        string extension = Path.GetExtension(fileName);

        if (string.IsNullOrEmpty(extension) || !Accepted.TryGetValue(extension, out var match))
        {
            return false;
        }

        (kind, contentType) = match;
        return true;
    }

    /// <summary>
    /// Whether a stored file is safe to render in the browser tab rather than downloaded.
    ///
    /// Deliberately narrow. A document served inline is a document the browser might decide to
    /// interpret, so only the types we actively want to preview are allowed to.
    /// </summary>
    public static bool IsSafeToDisplayInline(ResourceKind kind) =>
        kind is ResourceKind.Pdf or ResourceKind.Video or ResourceKind.Image;
}
