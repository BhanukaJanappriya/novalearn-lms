using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Common.Errors;

/// <summary>Central catalogue of course-content (module and lesson) failures.</summary>
public static class ContentErrors
{
    public static readonly Error ModuleNotFound =
        Error.NotFound("content.module_not_found", "The requested module was not found.");

    public static readonly Error LessonNotFound =
        Error.NotFound("content.lesson_not_found", "The requested lesson was not found.");

    public static readonly Error NotOwner =
        Error.Forbidden("content.not_owner", "You can only manage content for courses that you own.");

    public static readonly Error NotVisible =
        Error.Forbidden("content.not_visible", "This course's content is not available yet.");

    public static readonly Error Unauthenticated =
        Error.Unauthorized("content.unauthenticated", "You must be signed in to view or manage course content.");

    public static readonly Error InvalidOrder =
        Error.Validation("content.invalid_order", "The supplied order must list every item exactly once.");
}
