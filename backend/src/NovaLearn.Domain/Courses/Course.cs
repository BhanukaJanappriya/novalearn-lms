using NovaLearn.Domain.Common;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Domain.Courses;

/// <summary>
/// A course aggregate. Owned by the lecturer (or admin) who created it; auditing and soft-delete
/// are inherited from <see cref="BaseEntity"/>. Constructed through <see cref="Create"/> so
/// invariants (trimmed, normalised code, non-negative price) hold from the start.
/// </summary>
public sealed class Course : BaseEntity
{
    private Course() { } // EF Core

    public string Title { get; private set; } = null!;

    /// <summary>Short human code, e.g. "CS101". Unique and stored upper-cased.</summary>
    public string Code { get; private set; } = null!;

    public string? Description { get; private set; }

    public string Category { get; private set; } = null!;

    public CourseLevel Level { get; private set; }

    public CourseStatus Status { get; private set; }

    public decimal Price { get; private set; }

    public string? CoverImageUrl { get; private set; }

    /// <summary>The user (lecturer or admin) who owns this course.</summary>
    public Guid LecturerId { get; private set; }

    /// <summary>Optional navigation to the owning user (for read projections).</summary>
    public ApplicationUser? Lecturer { get; private set; }

    public static Course Create(
        string title,
        string code,
        string? description,
        string category,
        CourseLevel level,
        CourseStatus status,
        decimal price,
        string? coverImageUrl,
        Guid lecturerId)
    {
        return new Course
        {
            Title = title.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Category = category.Trim(),
            Level = level,
            Status = status,
            Price = price < 0 ? 0 : price,
            CoverImageUrl = string.IsNullOrWhiteSpace(coverImageUrl) ? null : coverImageUrl.Trim(),
            LecturerId = lecturerId
        };
    }

    public void Publish() => Status = CourseStatus.Published;

    public void Unpublish() => Status = CourseStatus.Draft;
}
