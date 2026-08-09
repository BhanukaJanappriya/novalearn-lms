using NovaLearn.Domain.Departments;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>Persistence port for the <see cref="Department"/> aggregate.</summary>
public interface IDepartmentRepository
{
    Task AddAsync(Department department, CancellationToken cancellationToken);

    /// <summary>Loads a department with its head, or null.</summary>
    Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>All departments, alphabetical, with heads included.</summary>
    Task<IReadOnlyList<Department>> ListAsync(CancellationToken cancellationToken);

    void Remove(Department department);

    /// <summary>
    /// Whether another live department already uses this (normalised) code.
    /// <paramref name="excludingId"/> lets an edit keep its own code.
    /// </summary>
    Task<bool> CodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken);

    /// <summary>How many courses sit under each department, keyed by department id.</summary>
    Task<IReadOnlyDictionary<Guid, int>> CountCoursesAsync(CancellationToken cancellationToken);
}
