using FluentAssertions;
using NovaLearn.Domain.Resources;
using Xunit;

namespace NovaLearn.Application.UnitTests.Resources;

public sealed class UploadedFileTypesTests
{
    [Theory]
    [InlineData("notes.pdf", ResourceKind.Pdf, "application/pdf")]
    [InlineData("lecture.mp4", ResourceKind.Video, "video/mp4")]
    [InlineData("diagram.PNG", ResourceKind.Image, "image/png")]
    [InlineData("slides.pptx", ResourceKind.Document,
        "application/vnd.openxmlformats-officedocument.presentationml.presentation")]
    public void An_accepted_file_resolves_to_its_kind_and_type(
        string fileName, ResourceKind expectedKind, string expectedContentType)
    {
        UploadedFileTypes.TryResolve(fileName, out ResourceKind kind, out string contentType)
            .Should().BeTrue();

        kind.Should().Be(expectedKind);
        contentType.Should().Be(expectedContentType);
    }

    [Theory]
    [InlineData("payload.exe")]
    [InlineData("script.js")]
    [InlineData("page.html")]
    [InlineData("shell.sh")]
    [InlineData("library.dll")]
    [InlineData("page.svg")]
    [InlineData("noextension")]
    [InlineData("")]
    public void Anything_not_on_the_allowlist_is_refused(string fileName)
    {
        // An allowlist rather than a blocklist: an unfamiliar extension is refused rather than
        // stored and worried about later. SVG is excluded deliberately, being script bearing.
        UploadedFileTypes.TryResolve(fileName, out _, out _).Should().BeFalse();
    }

    [Fact]
    public void A_double_extension_is_judged_by_its_last_part()
    {
        // "notes.pdf.exe" is an executable, whatever it is trying to look like.
        UploadedFileTypes.TryResolve("notes.pdf.exe", out _, out _).Should().BeFalse();
        UploadedFileTypes.TryResolve("archive.exe.pdf", out ResourceKind kind, out _).Should().BeTrue();
        kind.Should().Be(ResourceKind.Pdf);
    }

    [Theory]
    [InlineData(ResourceKind.Pdf, true)]
    [InlineData(ResourceKind.Video, true)]
    [InlineData(ResourceKind.Image, true)]
    [InlineData(ResourceKind.Document, false)]
    public void Only_previewable_kinds_may_be_served_inline(ResourceKind kind, bool expected)
    {
        // A document served inline is a document the browser might decide to interpret.
        UploadedFileTypes.IsSafeToDisplayInline(kind).Should().Be(expected);
    }
}
