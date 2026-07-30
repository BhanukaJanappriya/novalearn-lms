using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Content.Common;

/// <summary>
/// The single place that decides who may read or change a course's content, so the nine
/// content use cases do not each restate the rule. Mirrors the ownership check used by
/// <c>DeleteCourseCommandHandler</c>: the owning lecturer, or any administrator.
/// </summary>
public static class ContentAuthority
{
    /// <summary>
    /// Returns the error that should be surfaced, or <c>null</c> when the caller may edit
    /// the course's modules and lessons.
    /// </summary>
    public static Error? CheckCanManage(Course? course, ICurrentUser currentUser)
    {
        if (course is null)
        {
            return CourseErrors.NotFound;
        }

        if (currentUser.UserId is null)
        {
            return ContentErrors.Unauthenticated;
        }

        // Lecturers may only touch their own courses; admins may touch any.
        return IsAdmin(currentUser) || course.LecturerId == currentUser.UserId
            ? null
            : ContentErrors.NotOwner;
    }

    /// <summary>
    /// Returns the error that should be surfaced, or <c>null</c> when the caller may read the
    /// course's content. Published courses are readable by any signed-in user; anything not yet
    /// published is visible only to its owning lecturer and to administrators.
    /// </summary>
    public static Error? CheckCanRead(Course? course, ICurrentUser currentUser)
    {
        if (course is null)
        {
            return CourseErrors.NotFound;
        }

        if (currentUser.UserId is null)
        {
            return ContentErrors.Unauthenticated;
        }

        if (course.Status == CourseStatus.Published)
        {
            return null;
        }

        return IsAdmin(currentUser) || course.LecturerId == currentUser.UserId
            ? null
            : ContentErrors.NotVisible;
    }

    private static bool IsAdmin(ICurrentUser currentUser) =>
        currentUser.IsInRole(Roles.Administrator) || currentUser.IsInRole(Roles.SuperAdministrator);
}
