using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Resources;

namespace NovaLearn.Application.Features.Resources.Common;

/// <summary>
/// Who may post, change and see wall resources, in one place so the use cases cannot drift apart.
/// Mirrors <c>AssessmentAuthority</c> from the assessment slice.
/// </summary>
public static class ResourceAuthority
{
    /// <summary>Whether the caller is an administrator of any kind.</summary>
    public static bool IsAdmin(ICurrentUser currentUser) =>
        currentUser.IsInRole(Roles.Administrator) || currentUser.IsInRole(Roles.SuperAdministrator);

    /// <summary>Whether the caller may post at all. Enforced again at the endpoint by role.</summary>
    public static bool CanPost(ICurrentUser currentUser) =>
        IsAdmin(currentUser) || currentUser.IsInRole(Roles.Lecturer);

    /// <summary>
    /// Whether the caller may attach a post to this course.
    ///
    /// A lecturer may post to their own courses; an administrator to any. Posting to no course at
    /// all is open to anyone who may post, since that is the platform wide wall.
    /// </summary>
    public static bool CanPostToCourse(Course? course, ICurrentUser currentUser) =>
        course is not null
        && (IsAdmin(currentUser) || course.LecturerId == currentUser.UserId);

    /// <summary>
    /// Whether the caller may edit or remove an existing post: the person who posted it, or an
    /// administrator. A lecturer owning the course is deliberately not enough, so one member of
    /// staff cannot quietly rewrite another's material.
    /// </summary>
    public static bool CanManage(Resource resource, ICurrentUser currentUser) =>
        currentUser.UserId is { } callerId
        && (resource.PostedById == callerId || IsAdmin(currentUser));
}
