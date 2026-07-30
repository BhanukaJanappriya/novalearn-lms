using NovaLearn.Domain.Content;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>Persistence port for the <see cref="CourseModule"/> aggregate and its lessons.</summary>
public interface ICourseContentRepository
{
    /// <summary>A course's modules with their lessons, both ordered by <c>SortOrder</c>.</summary>
    Task<IReadOnlyList<CourseModule>> GetModulesForCourseAsync(Guid courseId, CancellationToken cancellationToken);

    /// <summary>Loads a module (with its course and lessons) by id, or null.</summary>
    Task<CourseModule?> GetModuleByIdAsync(Guid moduleId, CancellationToken cancellationToken);

    /// <summary>Loads a lesson with its module and that module's course, so ownership can be checked in one round trip.</summary>
    Task<Lesson?> GetLessonByIdAsync(Guid lessonId, CancellationToken cancellationToken);

    /// <summary>The lessons of a module, ordered by <c>SortOrder</c>.</summary>
    Task<IReadOnlyList<Lesson>> GetLessonsForModuleAsync(Guid moduleId, CancellationToken cancellationToken);

    /// <summary>The next free module position in a course, so new modules land at the end.</summary>
    Task<int> NextModuleSortOrderAsync(Guid courseId, CancellationToken cancellationToken);

    /// <summary>The next free lesson position in a module, so new lessons land at the end.</summary>
    Task<int> NextLessonSortOrderAsync(Guid moduleId, CancellationToken cancellationToken);

    Task AddModuleAsync(CourseModule module, CancellationToken cancellationToken);

    /// <summary>
    /// Tracks a lesson created through <see cref="CourseModule.AddLesson"/> as an insert.
    /// <see cref="Domain.Common.BaseEntity"/> assigns the key client-side, so a lesson reached
    /// only through the module's navigation is tracked as Modified and saves as a no-op UPDATE.
    /// The insert has to be stated explicitly.
    /// </summary>
    Task AddLessonAsync(Lesson lesson, CancellationToken cancellationToken);

    /// <summary>Soft-deletes a module and cascades the soft delete to its lessons.</summary>
    void RemoveModule(CourseModule module);

    void RemoveLesson(Lesson lesson);
}
