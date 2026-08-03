using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.Common;

/// <summary>
/// Who may author quizzes, in one place so the use cases cannot drift apart. Mirrors
/// <c>AssessmentAuthority</c>; each check returns the offending <see cref="Error"/>, or null.
/// </summary>
public static class QuizAuthority
{
    public static bool IsAdmin(ICurrentUser currentUser) =>
        currentUser.IsInRole(Roles.Administrator) || currentUser.IsInRole(Roles.SuperAdministrator);

    /// <summary>Authoring and results: the owning lecturer, or an administrator.</summary>
    public static Error? CheckCanManage(Course? course, ICurrentUser currentUser)
    {
        if (currentUser.UserId is not { } callerId)
        {
            return QuizErrors.Unauthenticated;
        }

        if (course is null)
        {
            return QuizErrors.CourseNotFound;
        }

        if (course.LecturerId == callerId || IsAdmin(currentUser))
        {
            return null;
        }

        return QuizErrors.NotCourseOwner;
    }
}
