namespace NovaLearn.Application.Features.Enrollments.Common;

/// <summary>
/// A published course as seen by a prospective learner. Distinct from the management-side
/// <c>CourseDto</c>: it omits the publication status and adds the caller-relative
/// <see cref="IsEnrolled"/> flag plus the course's <see cref="EnrolledCount"/>.
/// </summary>
public sealed record CourseCatalogDto(
    Guid Id,
    string Title,
    string Code,
    string? Description,
    string Category,
    string Level,
    decimal Price,
    string? CoverImageUrl,
    Guid LecturerId,
    string LecturerName,
    int EnrolledCount,
    bool IsEnrolled,
    DateTimeOffset CreatedAtUtc);
