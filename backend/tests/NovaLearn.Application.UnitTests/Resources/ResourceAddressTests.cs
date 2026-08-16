using FluentAssertions;
using NovaLearn.Domain.Resources;
using Xunit;

namespace NovaLearn.Application.UnitTests.Resources;

public sealed class ResourceAddressTests
{
    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("http://youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://m.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/shorts/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/live/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ")]
    public void Every_shape_of_youtube_address_yields_the_video_id(string url)
    {
        ResourceAddress.TryGetYouTubeVideoId(url, out string? videoId).Should().BeTrue();
        videoId.Should().Be("dQw4w9WgXcQ");
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=PL123&index=4")]
    [InlineData("https://www.youtube.com/watch?list=PL123&v=dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ?t=42")]
    public void A_playlist_or_a_start_time_is_not_mistaken_for_the_video(string url)
    {
        // Splitting on "v=" would happily return the playlist id here, which is why the query is
        // parsed into pairs rather than searched.
        ResourceAddress.TryGetYouTubeVideoId(url, out string? videoId).Should().BeTrue();
        videoId.Should().Be("dQw4w9WgXcQ");
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=tooshort")]
    [InlineData("https://www.youtube.com/watch?v=waytoolongtobeanid")]
    [InlineData("https://www.youtube.com/watch?v=has spaces!!")]
    [InlineData("https://www.youtube.com")]
    [InlineData("https://www.youtube.com/feed/subscriptions")]
    [InlineData("https://notyoutube.com/watch?v=dQw4w9WgXcQ")]
    public void An_address_without_a_real_video_id_is_not_treated_as_youtube(string url)
    {
        ResourceAddress.TryGetYouTubeVideoId(url, out string? videoId).Should().BeFalse();
        videoId.Should().BeNull();
    }

    [Fact]
    public void A_lookalike_host_does_not_pass_as_youtube()
    {
        // youtube.com.evil.test ends with the real host but is not it.
        ResourceAddress
            .TryGetYouTubeVideoId("https://youtube.com.evil.test/watch?v=dQw4w9WgXcQ", out string? videoId)
            .Should().BeFalse();

        videoId.Should().BeNull();
    }

    [Theory]
    [InlineData("https://drive.google.com/file/d/abc/view", ResourceKind.Drive)]
    [InlineData("https://docs.google.com/document/d/abc/edit", ResourceKind.Drive)]
    [InlineData("https://example.com/notes.pdf", ResourceKind.Link)]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ", ResourceKind.YouTube)]
    public void An_address_is_classified_by_where_it_points(string url, ResourceKind expected)
    {
        ResourceAddress.Classify(url).Kind.Should().Be(expected);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html;base64,PHNjcmlwdD4=")]
    [InlineData("file:///etc/passwd")]
    [InlineData("/relative/path.png")]
    [InlineData("notaurl")]
    [InlineData("")]
    [InlineData(null)]
    public void Only_http_and_https_addresses_are_usable(string? url)
    {
        // Anything else would be handed straight back to every browser rendering the wall.
        ResourceAddress.IsUsable(url).Should().BeFalse();
    }

    [Fact]
    public void The_thumbnail_and_embed_addresses_are_derived_from_the_video_id()
    {
        ResourceAddress.Classify("https://youtu.be/dQw4w9WgXcQ").YouTubeVideoId
            .Should().Be("dQw4w9WgXcQ");

        ResourceAddress.YouTubeThumbnailUrl("dQw4w9WgXcQ")
            .Should().Be("https://img.youtube.com/vi/dQw4w9WgXcQ/hqdefault.jpg");

        // The embed uses the no-cookie host, so viewing the wall does not set YouTube cookies.
        ResourceAddress.YouTubeEmbedUrl("dQw4w9WgXcQ")
            .Should().Be("https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ");
    }

    [Fact]
    public void There_is_no_thumbnail_when_there_is_no_video()
    {
        ResourceAddress.YouTubeThumbnailUrl(null).Should().BeNull();
        ResourceAddress.YouTubeEmbedUrl(null).Should().BeNull();
    }
}
