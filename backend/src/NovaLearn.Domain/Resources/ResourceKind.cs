namespace NovaLearn.Domain.Resources;

/// <summary>
/// What a posted resource actually is, which decides how the wall renders it.
///
/// The kind is worked out from the file or the address rather than chosen by whoever posts it, so
/// a YouTube link cannot be filed as a PDF and then fail to render.
/// </summary>
public enum ResourceKind
{
    /// <summary>An uploaded PDF, typically notes.</summary>
    Pdf,

    /// <summary>An uploaded video file, played inline.</summary>
    Video,

    /// <summary>An uploaded image, shown inline.</summary>
    Image,

    /// <summary>Any other uploaded document: slides, a spreadsheet, an archive.</summary>
    Document,

    /// <summary>A YouTube address. The only kind with a thumbnail we can derive for free.</summary>
    YouTube,

    /// <summary>A Google Drive or Docs address.</summary>
    Drive,

    /// <summary>Any other external address.</summary>
    Link
}
