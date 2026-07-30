using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Content;

namespace NovaLearn.Persistence.Repositories;

public sealed class CourseContentRepository(ApplicationDbContext dbContext) : ICourseContentRepository
{
    public async Task<IReadOnlyList<CourseModule>> GetModulesForCourseAsync(
        Guid courseId, CancellationToken cancellationToken) =>
        await dbContext.CourseModules
            .Where(m => m.CourseId == courseId)
            .Include(m => m.Lessons.OrderBy(l => l.SortOrder))
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);

    public Task<CourseModule?> GetModuleByIdAsync(Guid moduleId, CancellationToken cancellationToken) =>
        dbContext.CourseModules
            .Include(m => m.Course)
            .Include(m => m.Lessons.OrderBy(l => l.SortOrder))
            .FirstOrDefaultAsync(m => m.Id == moduleId, cancellationToken);

    public Task<Lesson?> GetLessonByIdAsync(Guid lessonId, CancellationToken cancellationToken) =>
        dbContext.Lessons
            .Include(l => l.Module)
            .ThenInclude(m => m!.Course)
            .FirstOrDefaultAsync(l => l.Id == lessonId, cancellationToken);

    public async Task<IReadOnlyList<Lesson>> GetLessonsForModuleAsync(
        Guid moduleId, CancellationToken cancellationToken) =>
        await dbContext.Lessons
            .Where(l => l.ModuleId == moduleId)
            .OrderBy(l => l.SortOrder)
            .ToListAsync(cancellationToken);

    public async Task<int> NextModuleSortOrderAsync(Guid courseId, CancellationToken cancellationToken)
    {
        // Max() over an empty set needs a nullable projection, hence the cast.
        int? highest = await dbContext.CourseModules
            .Where(m => m.CourseId == courseId)
            .MaxAsync(m => (int?)m.SortOrder, cancellationToken);

        return (highest ?? -1) + 1;
    }

    public async Task<int> NextLessonSortOrderAsync(Guid moduleId, CancellationToken cancellationToken)
    {
        int? highest = await dbContext.Lessons
            .Where(l => l.ModuleId == moduleId)
            .MaxAsync(l => (int?)l.SortOrder, cancellationToken);

        return (highest ?? -1) + 1;
    }

    public async Task AddModuleAsync(CourseModule module, CancellationToken cancellationToken) =>
        await dbContext.CourseModules.AddAsync(module, cancellationToken);

    public async Task AddLessonAsync(Lesson lesson, CancellationToken cancellationToken) =>
        await dbContext.Lessons.AddAsync(lesson, cancellationToken);

    public void RemoveModule(CourseModule module)
    {
        // The database cascade only fires on a hard delete; because the interceptor turns these
        // into soft deletes, the children have to be marked explicitly. Copy the collection
        // first, since EF fixes up the navigation as each lesson changes state.
        foreach (Lesson lesson in module.Lessons.ToList())
        {
            dbContext.Lessons.Remove(lesson);
        }

        dbContext.CourseModules.Remove(module);
    }

    public void RemoveLesson(Lesson lesson) => dbContext.Lessons.Remove(lesson);
}
