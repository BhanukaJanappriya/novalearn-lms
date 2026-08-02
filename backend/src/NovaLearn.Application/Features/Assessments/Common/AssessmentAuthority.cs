using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Assessments.Common;

/// <summary>
/// Who may do what with a course's assessments, in one place so the nine use cases cannot
/// drift apart. Each check returns the offending <see cref="Error"/>, or null when allowed.
///
/// Mirrors <c>ContentAuthority</c> from the content slice: authoring is limited to the owning
/// lecturer or an administrator, while reading follows publication state.
/// </summary>
public static class AssessmentAuthority
{
    /// <summary>Whether the caller is an administrator of any kind.</summary>
    public static bool IsAdmin(ICurrentUser currentUser) =>
        currentUser.IsInRole(Roles.Administrator) || currentUser.IsInRole(Roles.SuperAdministrator);

    /// <summary>
    /// Authoring and marking: the owning lecturer, or an administrator. A null course means the
    /// navigation was not loaded, which is a programming error rather than a permission one.
    /// </summary>
    public static Error? CheckCanManage(Course? course, ICurrentUser currentUser)
    {
        if (currentUser.UserId is not { } callerId)
        {
            return AssessmentErrors.Unauthenticated;
        }

        if (course is null)
        {
            return AssessmentErrors.CourseNotFound;
        }

        if (course.LecturerId == callerId || IsAdmin(currentUser))
        {
            return null;
        }

        return AssessmentErrors.NotCourseOwner;
    }
}
