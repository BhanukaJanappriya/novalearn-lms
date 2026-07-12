using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Common.Errors;

/// <summary>Central catalogue of course-related failures.</summary>
public static class CourseErrors
{
    public static readonly Error NotFound =
        Error.NotFound("course.not_found", "The requested course was not found.");

    public static readonly Error DuplicateCode =
        Error.Conflict("course.duplicate_code", "A course with this code already exists.");

    public static readonly Error NotOwner =
        Error.Forbidden("course.not_owner", "You can only delete courses that you own.");

    public static readonly Error Unauthenticated =
        Error.Unauthorized("course.unauthenticated", "You must be signed in to manage courses.");
}
