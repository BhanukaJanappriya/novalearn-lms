using NovaLearn.Domain.Common;
using NovaLearn.Domain.Courses;

namespace NovaLearn.Domain.Content;

/// <summary>
/// A chapter of a course and the aggregate root for its <see cref="Lesson"/> children.
/// Constructed through <see cref="Create"/> so invariants (trimmed strings, non-negative
/// ordering) hold from the start.
/// </summary>
public sealed class CourseModule : BaseEntity
{
    private readonly List<Lesson> _lessons = [];

    private CourseModule() { } // EF Core

    public Guid CourseId { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    /// <summary>Position within the course. Zero-based and never negative.</summary>
    public int SortOrder { get; private set; }

    /// <summary>Navigation back to the owning course (used for ownership checks).</summary>
    public Course? Course { get; private set; }

    /// <summary>The module's lessons. Mutate through <see cref="AddLesson"/>, never this collection.</summary>
    public IReadOnlyCollection<Lesson> Lessons => _lessons.AsReadOnly();

    public static CourseModule Create(Guid courseId, string title, string? description, int sortOrder) =>
        new()
        {
            CourseId = courseId,
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            SortOrder = sortOrder < 0 ? 0 : sortOrder
        };

    /// <summary>Applies edited details, keeping the same invariants as <see cref="Create"/>.</summary>
    public void Update(string title, string? description)
    {
        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void MoveTo(int sortOrder) => SortOrder = sortOrder < 0 ? 0 : sortOrder;

    /// <summary>Creates a lesson under this module and attaches it to the aggregate.</summary>
    public Lesson AddLesson(
        string title,
        LessonType type,
        string? contentUrl,
        string? textContent,
        int? durationMinutes,
        int sortOrder,
        bool isPreview)
    {
        Lesson lesson = Lesson.Create(
            Id, title, type, contentUrl, textContent, durationMinutes, sortOrder, isPreview);

        _lessons.Add(lesson);
        return lesson;
    }

    /// <summary>The next free lesson position, so appended lessons land at the end.</summary>
    public int NextLessonSortOrder() => _lessons.Count == 0 ? 0 : _lessons.Max(l => l.SortOrder) + 1;
}
