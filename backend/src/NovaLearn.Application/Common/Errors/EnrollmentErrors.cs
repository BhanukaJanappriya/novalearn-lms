using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Common.Errors;

/// <summary>Central catalogue of enrolment-related failures.</summary>
public static class EnrollmentErrors
{
    public static readonly Error NotFound =
        Error.NotFound("enrollment.not_found", "The requested enrollment was not found.");

    public static readonly Error AlreadyEnrolled =
        Error.Conflict("enrollment.already_enrolled", "You are already enrolled in this course.");

    public static readonly Error CourseNotPublished =
        Error.Conflict("enrollment.course_not_published", "This course is not open for enrollment yet.");

    public static readonly Error NotOwner =
        Error.Forbidden("enrollment.not_owner", "You can only manage your own enrollments.");

    public static readonly Error NotCourseOwner =
        Error.Forbidden("enrollment.not_course_owner", "You can only view the roster for courses that you own.");

    public static readonly Error Unauthenticated =
        Error.Unauthorized("enrollment.unauthenticated", "You must be signed in to manage enrollments.");
}
