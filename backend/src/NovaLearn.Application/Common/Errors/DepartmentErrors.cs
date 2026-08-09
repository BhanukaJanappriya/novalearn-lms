using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Common.Errors;

/// <summary>Central catalogue of department failures.</summary>
public static class DepartmentErrors
{
    public static readonly Error NotFound =
        Error.NotFound("department.not_found", "The requested department was not found.");

    public static readonly Error CodeInUse =
        Error.Conflict("department.code_in_use", "Another department already uses this code.");

    public static readonly Error HasCourses =
        Error.Conflict(
            "department.has_courses",
            "This department still has courses. Move or reassign them first, or retire the department instead.");

    public static readonly Error HeadNotALecturer =
        Error.Validation(
            "department.head_not_a_lecturer",
            "A department head must be a lecturer or an administrator.");
}
