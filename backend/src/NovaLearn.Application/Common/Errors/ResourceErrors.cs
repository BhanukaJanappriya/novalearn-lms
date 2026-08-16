using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Common.Errors;

/// <summary>Central catalogue of failures when posting or reading wall resources.</summary>
public static class ResourceErrors
{
    public static readonly Error NotFound =
        Error.NotFound("resource.not_found", "The requested resource was not found.");

    public static readonly Error FileNotFound =
        Error.NotFound("resource.file_not_found", "The file behind this resource is missing.");

    public static readonly Error Unauthenticated =
        Error.Unauthorized("resource.unauthenticated", "You must be signed in to use the wall.");

    public static readonly Error NotPoster =
        Error.Forbidden("resource.not_poster", "Only the person who posted this, or an administrator, can change it.");

    public static readonly Error NotCourseOwner =
        Error.Forbidden(
            "resource.not_course_owner",
            "You can only post to a course you teach.");

    public static readonly Error NotVisible =
        Error.Forbidden("resource.not_visible", "This resource belongs to a course you are not on.");

    public static readonly Error CourseNotFound =
        Error.NotFound("resource.course_not_found", "The course this was posted to was not found.");

    public static readonly Error InvalidLink =
        Error.Validation("resource.invalid_link", "A link must be an absolute http or https address.");

    public static readonly Error EmptyFile =
        Error.Validation("resource.empty_file", "The uploaded file is empty.");

    public static readonly Error UnsupportedFileType =
        Error.Validation(
            "resource.unsupported_file_type",
            "That file type is not accepted. Upload a PDF, video, image or document.");

    public static Error FileTooLarge(int maxMegabytes) =>
        Error.Validation(
            "resource.file_too_large",
            $"That file is larger than the {maxMegabytes} MB limit.");
}
