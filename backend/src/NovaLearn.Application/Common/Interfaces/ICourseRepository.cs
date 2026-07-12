using NovaLearn.Domain.Courses;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>Persistence port for the <see cref="Course"/> aggregate.</summary>
public interface ICourseRepository
{
    Task AddAsync(Course course, CancellationToken cancellationToken);

    /// <summary>Loads a course (with its <see cref="Course.Lecturer"/>) by id, or null.</summary>
    Task<Course?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Remove(Course course);

    /// <summary>Whether a course already uses the given (normalised, upper-cased) code.</summary>
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken);

    /// <summary>All courses, newest first, with the owning lecturer included.</summary>
    Task<IReadOnlyList<Course>> ListAsync(CancellationToken cancellationToken);
}
