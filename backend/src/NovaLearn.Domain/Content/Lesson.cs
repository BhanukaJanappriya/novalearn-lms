using NovaLearn.Domain.Common;

namespace NovaLearn.Domain.Content;

/// <summary>
/// A single unit of study inside a <see cref="CourseModule"/>. Lessons are added through their
/// owning module so the aggregate stays in charge of its children; invariants (trimmed strings,
/// non-negative ordering, content matching the type) hold from the start.
/// </summary>
public sealed class Lesson : BaseEntity
{
    private Lesson() { } // EF Core

    public Guid ModuleId { get; private set; }

    public string Title { get; private set; } = null!;

    public LessonType Type { get; private set; }

    /// <summary>Where the material lives, for <see cref="LessonType.Video"/>, <see cref="LessonType.Pdf"/> and <see cref="LessonType.Link"/>.</summary>
    public string? ContentUrl { get; private set; }

    /// <summary>Inline body, used only by <see cref="LessonType.Text"/> lessons.</summary>
    public string? TextContent { get; private set; }

    /// <summary>Estimated time to complete, in whole minutes. Never negative.</summary>
    public int? DurationMinutes { get; private set; }

    /// <summary>Position within the module. Zero-based and never negative.</summary>
    public int SortOrder { get; private set; }

    /// <summary>Whether this lesson is a free preview, readable without enrolling.</summary>
    public bool IsPreview { get; private set; }

    /// <summary>Navigation back to the owning module.</summary>
    public CourseModule? Module { get; private set; }

    public static Lesson Create(
        Guid moduleId,
        string title,
        LessonType type,
        string? contentUrl,
        string? textContent,
        int? durationMinutes,
        int sortOrder,
        bool isPreview)
    {
        var lesson = new Lesson
        {
            ModuleId = moduleId,
            Title = title.Trim(),
            SortOrder = sortOrder < 0 ? 0 : sortOrder,
            DurationMinutes = Normalise(durationMinutes),
            IsPreview = isPreview
        };

        lesson.ApplyContent(type, contentUrl, textContent);
        return lesson;
    }

    /// <summary>Applies edited details, keeping the same invariants as <see cref="Create"/>.</summary>
    public void Update(
        string title,
        LessonType type,
        string? contentUrl,
        string? textContent,
        int? durationMinutes,
        bool isPreview)
    {
        Title = title.Trim();
        DurationMinutes = Normalise(durationMinutes);
        IsPreview = isPreview;
        ApplyContent(type, contentUrl, textContent);
    }

    public void MoveTo(int sortOrder) => SortOrder = sortOrder < 0 ? 0 : sortOrder;

    public void SetPreview(bool isPreview) => IsPreview = isPreview;

    /// <summary>
    /// Keeps content fields consistent with the type: a text lesson carries only
    /// <see cref="TextContent"/>, every other type carries only <see cref="ContentUrl"/>.
    /// </summary>
    private void ApplyContent(LessonType type, string? contentUrl, string? textContent)
    {
        Type = type;

        if (type == LessonType.Text)
        {
            TextContent = Clean(textContent);
            ContentUrl = null;
        }
        else
        {
            ContentUrl = Clean(contentUrl);
            TextContent = null;
        }
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int? Normalise(int? durationMinutes) =>
        durationMinutes is null ? null : Math.Max(0, durationMinutes.Value);
}
