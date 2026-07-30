using NovaLearn.Domain.Content;
using NovaLearn.Domain.Courses;

namespace NovaLearn.Application.UnitTests.Content;

/// <summary>Small builders so the content handler tests stay about behaviour, not setup.</summary>
internal static class ContentTestData
{
    public static Course NewCourse(Guid lecturerId, CourseStatus status = CourseStatus.Published) =>
        Course.Create(
            "Intro to Programming",
            "CS101",
            "A first course.",
            "Computer Science",
            CourseLevel.Beginner,
            status,
            0m,
            null,
            lecturerId);

    /// <summary>
    /// Attaches the <see cref="CourseModule.Course"/> navigation that the repository populates
    /// with an <c>Include</c>. The setter is private by design, so the test reaches it the same
    /// way EF Core does.
    /// </summary>
    public static CourseModule WithCourse(this CourseModule module, Course course)
    {
        typeof(CourseModule)
            .GetProperty(nameof(CourseModule.Course))!
            .SetValue(module, course);

        return module;
    }
}
