using FluentAssertions;
using NovaLearn.Domain.Content;
using Xunit;

namespace NovaLearn.Application.UnitTests.Content;

public sealed class LessonTests
{
    private static Lesson NewLesson(LessonType type, string? contentUrl, string? textContent) =>
        Lesson.Create(Guid.NewGuid(), " Variables ", type, contentUrl, textContent, 12, 0, isPreview: false);

    [Fact]
    public void Text_lesson_keeps_its_body_and_drops_any_url()
    {
        Lesson lesson = NewLesson(LessonType.Text, "https://videos.local/clip.mp4", " Some prose. ");

        lesson.TextContent.Should().Be("Some prose.");
        lesson.ContentUrl.Should().BeNull();
    }

    [Fact]
    public void Video_lesson_keeps_its_url_and_drops_any_body()
    {
        Lesson lesson = NewLesson(LessonType.Video, " https://videos.local/clip.mp4 ", "Some prose.");

        lesson.ContentUrl.Should().Be("https://videos.local/clip.mp4");
        lesson.TextContent.Should().BeNull();
    }

    [Fact]
    public void Changing_the_type_swaps_which_content_field_is_kept()
    {
        Lesson lesson = NewLesson(LessonType.Text, null, "Some prose.");

        lesson.Update(
            "Variables",
            LessonType.Video,
            "https://videos.local/clip.mp4",
            "Some prose.",
            durationMinutes: 5,
            isPreview: true);

        lesson.ContentUrl.Should().Be("https://videos.local/clip.mp4");
        lesson.TextContent.Should().BeNull();
        lesson.IsPreview.Should().BeTrue();
    }

    [Fact]
    public void Create_trims_the_title_and_clamps_negative_numbers()
    {
        Lesson lesson = Lesson.Create(
            Guid.NewGuid(), "  Variables  ", LessonType.Link, "https://docs.local", null,
            durationMinutes: -5, sortOrder: -3, isPreview: false);

        lesson.Title.Should().Be("Variables");
        lesson.DurationMinutes.Should().Be(0);
        lesson.SortOrder.Should().Be(0);
    }

    [Fact]
    public void A_blank_body_becomes_null_rather_than_whitespace()
    {
        Lesson lesson = NewLesson(LessonType.Text, null, "   ");

        lesson.TextContent.Should().BeNull();
    }
}
