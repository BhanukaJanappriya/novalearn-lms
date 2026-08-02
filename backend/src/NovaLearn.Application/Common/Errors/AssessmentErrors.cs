using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Common.Errors;

/// <summary>Central catalogue of assessment failures.</summary>
public static class AssessmentErrors
{
    public static readonly Error AssignmentNotFound =
        Error.NotFound("assessment.assignment_not_found", "The requested assignment was not found.");

    public static readonly Error SubmissionNotFound =
        Error.NotFound("assessment.submission_not_found", "The requested submission was not found.");

    public static readonly Error CourseNotFound =
        Error.NotFound("assessment.course_not_found", "The requested course was not found.");

    public static readonly Error Unauthenticated =
        Error.Unauthorized("assessment.unauthenticated", "You must be signed in to work with assessments.");

    public static readonly Error NotCourseOwner =
        Error.Forbidden(
            "assessment.not_course_owner",
            "You can only manage assessments for courses that you own.");

    public static readonly Error NotEnrolled =
        Error.Forbidden("assessment.not_enrolled", "You must be enrolled in this course to submit work.");

    public static readonly Error NotSubmissionOwner =
        Error.Forbidden("assessment.not_submission_owner", "You can only view your own submission.");

    public static readonly Error NotOpen =
        Error.Conflict(
            "assessment.not_open",
            "This assignment is not open for submission. The due date may have passed.");

    public static readonly Error AssignmentNotPublished =
        Error.Forbidden("assessment.not_published", "This assignment has not been published yet.");
}
