using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Departments;

namespace NovaLearn.Persistence.Repositories;

public sealed class DepartmentRepository(ApplicationDbContext dbContext) : IDepartmentRepository
{
    public async Task AddAsync(Department department, CancellationToken cancellationToken) =>
        await dbContext.Departments.AddAsync(department, cancellationToken);

    public Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Departments
            .Include(d => d.Head)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Department>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Departments
            .Include(d => d.Head)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);

    public void Remove(Department department) => dbContext.Departments.Remove(department);

    public Task<bool> CodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken)
    {
        string normalised = code.Trim().ToUpperInvariant();

        return dbContext.Departments
            .AnyAsync(
                d => d.Code == normalised && (excludingId == null || d.Id != excludingId),
                cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> CountCoursesAsync(CancellationToken cancellationToken)
    {
        var counts = await dbContext.Courses
            .Where(c => c.DepartmentId != null)
            .GroupBy(c => c.DepartmentId!.Value)
            .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(c => c.DepartmentId, c => c.Count);
    }
}
