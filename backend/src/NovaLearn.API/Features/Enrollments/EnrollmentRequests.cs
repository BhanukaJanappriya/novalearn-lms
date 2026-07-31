using NovaLearn.Domain.Courses;

namespace NovaLearn.API.Features.Enrollments;

/// <summary>
/// Query string for the learner-facing catalogue. Enums accept their string names
/// (e.g. "Beginner"); all members are optional.
/// </summary>
public sealed record CourseCatalogRequest
{
    /// <summary>Case-insensitive match against the course title or code.</summary>
    public string? Search { get; init; }

    public string? Category { get; init; }

    public CourseLevel? Level { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 12;
}

/// <summary>Body for recording progress through a course.</summary>
/// <param name="ProgressPercent">Completion percentage, 0 to 100. Reaching 100 completes the enrolment.</param>
public sealed record UpdateProgressRequest(int ProgressPercent);
