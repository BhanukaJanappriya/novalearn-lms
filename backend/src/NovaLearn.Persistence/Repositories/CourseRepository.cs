using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Courses;

namespace NovaLearn.Persistence.Repositories;

public sealed class CourseRepository(ApplicationDbContext dbContext) : ICourseRepository
{
    public async Task AddAsync(Course course, CancellationToken cancellationToken) =>
        await dbContext.Courses.AddAsync(course, cancellationToken);

    public Task<Course?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Courses
            .Include(c => c.Lecturer)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public void Remove(Course course) => dbContext.Courses.Remove(course);

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken) =>
        dbContext.Courses.AnyAsync(c => c.Code == code, cancellationToken);

    public async Task<IReadOnlyList<Course>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Courses
            .Include(c => c.Lecturer)
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);
}
