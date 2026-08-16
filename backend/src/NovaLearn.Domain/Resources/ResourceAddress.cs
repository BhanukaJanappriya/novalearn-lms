using System.Text.RegularExpressions;

namespace NovaLearn.Domain.Resources;

/// <summary>
/// Reads an external address and works out what it points at.
///
/// This lives in the domain rather than in the client because both the wall and the API have to
/// agree about what a link is. Deciding it once, on the way in, also means the answer is stored
/// with the row instead of being re-derived by every reader.
/// </summary>
public static class ResourceAddress
{
    /// <summary>A YouTube id is eleven characters of a restricted alphabet, and nothing else.</summary>
    private static readonly Regex VideoIdPattern = new(
        "^[A-Za-z0-9_-]{11}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly string[] YouTubeHosts =
        ["youtube.com", "www.youtube.com", "m.youtube.com", "music.youtube.com",
         "youtube-nocookie.com", "www.youtube-nocookie.com"];

    private static readonly string[] ShortYouTubeHosts = ["youtu.be", "www.youtu.be"];

    private static readonly string[] DriveHosts =
        ["drive.google.com", "docs.google.com", "sheets.google.com", "slides.google.com"];

    /// <summary>
    /// Whether an address is one we are willing to store. Only http and https: a
    /// <c>javascript:</c> or <c>data:</c> address would otherwise be handed straight back to every
    /// browser rendering the wall.
    /// </summary>
    public static bool IsUsable(string? url) =>
        TryParse(url, out _);

    private static bool TryParse(string? url, out Uri parsed)
    {
        parsed = null!;

        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out Uri? candidate))
        {
            return false;
        }

        if (candidate.Scheme != Uri.UriSchemeHttp && candidate.Scheme != Uri.UriSchemeHttps)
        {
            return false;
        }

        parsed = candidate;
        return true;
    }

    /// <summary>
    /// Classifies an address, returning the YouTube video id when there is one.
    /// </summary>
    public static (ResourceKind Kind, string? YouTubeVideoId) Classify(string url)
    {
        if (!TryParse(url, out Uri parsed))
        {
            return (ResourceKind.Link, null);
        }

        if (TryGetYouTubeVideoId(parsed, out string? videoId))
        {
            return (ResourceKind.YouTube, videoId);
        }

        string host = parsed.Host.ToLowerInvariant();

        return DriveHosts.Contains(host)
            ? (ResourceKind.Drive, null)
            : (ResourceKind.Link, null);
    }

    /// <summary>
    /// Pulls the video id out of the several shapes a YouTube address comes in: the usual
    /// <c>watch?v=</c>, the <c>youtu.be</c> short form, and the <c>embed</c>, <c>shorts</c> and
    /// <c>live</c> paths. Extra query parameters such as a playlist or a start time are ignored.
    /// </summary>
    public static bool TryGetYouTubeVideoId(string url, out string? videoId)
    {
        videoId = null;
        return TryParse(url, out Uri parsed) && TryGetYouTubeVideoId(parsed, out videoId);
    }

    private static bool TryGetYouTubeVideoId(Uri parsed, out string? videoId)
    {
        videoId = null;
        string host = parsed.Host.ToLowerInvariant();

        if (ShortYouTubeHosts.Contains(host))
        {
            return Accept(FirstSegment(parsed), out videoId);
        }

        if (!YouTubeHosts.Contains(host))
        {
            return false;
        }

        string[] segments = Segments(parsed);

        // /watch?v=ID, parsed by hand so a playlist or a start time cannot be mistaken for the id.
        if (segments is ["watch"])
        {
            foreach (string pair in parsed.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = pair.Split('=', 2);

                if (parts.Length == 2 && parts[0] == "v")
                {
                    return Accept(Uri.UnescapeDataString(parts[1]), out videoId);
                }
            }

            return false;
        }

        if (segments.Length >= 2 && segments[0] is "embed" or "shorts" or "live" or "v")
        {
            return Accept(segments[1], out videoId);
        }

        return false;
    }

    private static string[] Segments(Uri parsed) =>
        parsed.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

    private static string FirstSegment(Uri parsed) =>
        Segments(parsed).FirstOrDefault() ?? string.Empty;

    private static bool Accept(string candidate, out string? videoId)
    {
        videoId = VideoIdPattern.IsMatch(candidate) ? candidate : null;
        return videoId is not null;
    }

    /// <summary>
    /// The still image YouTube publishes for a video. Derived rather than fetched, so posting a
    /// link never depends on an outbound call from the server.
    /// </summary>
    public static string? YouTubeThumbnailUrl(string? videoId) =>
        videoId is null ? null : $"https://img.youtube.com/vi/{videoId}/hqdefault.jpg";

    /// <summary>The privacy respecting embed address for a video, used by the inline player.</summary>
    public static string? YouTubeEmbedUrl(string? videoId) =>
        videoId is null ? null : $"https://www.youtube-nocookie.com/embed/{videoId}";
}
