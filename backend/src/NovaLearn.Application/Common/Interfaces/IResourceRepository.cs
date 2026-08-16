using NovaLearn.Domain.Resources;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>Persistence port for the <see cref="Resource"/> aggregate.</summary>
public interface IResourceRepository
{
    Task AddAsync(Resource resource, CancellationToken cancellationToken);

    /// <summary>Loads a resource with its course and poster, or null.</summary>
    Task<Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Remove(Resource resource);

    /// <summary>
    /// The wall, newest first.
    /// </summary>
    /// <param name="visibleCourseIds">
    /// Courses the caller is on or teaches. Posts attached to any other course are left out;
    /// posts attached to no course are always included, being platform wide.
    /// </param>
    /// <param name="seesEverything">
    /// True for administrators, for whom <paramref name="visibleCourseIds"/> is ignored.
    /// </param>
    Task<IReadOnlyList<Resource>> ListWallAsync(
        IReadOnlyCollection<Guid> visibleCourseIds,
        bool seesEverything,
        Guid? courseId,
        ResourceKind? kind,
        string? search,
        CancellationToken cancellationToken);

    /// <summary>Course ids the caller may see posts for: enrolled on, or teaching.</summary>
    Task<IReadOnlyList<Guid>> VisibleCourseIdsAsync(Guid userId, CancellationToken cancellationToken);
}
