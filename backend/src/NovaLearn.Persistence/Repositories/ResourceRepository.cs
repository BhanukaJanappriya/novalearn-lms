using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Resources;

namespace NovaLearn.Persistence.Repositories;

/// <summary>EF Core implementation of the resource port.</summary>
internal sealed class ResourceRepository(ApplicationDbContext context) : IResourceRepository
{
    public async Task AddAsync(Resource resource, CancellationToken cancellationToken) =>
        await context.Resources.AddAsync(resource, cancellationToken);

    public Task<Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Resources
            .Include(r => r.Course)
            .Include(r => r.PostedBy)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public void Remove(Resource resource) => context.Resources.Remove(resource);

    public async Task<IReadOnlyList<Resource>> ListWallAsync(
        IReadOnlyCollection<Guid> visibleCourseIds,
        bool seesEverything,
        Guid? courseId,
        ResourceKind? kind,
        string? search,
        CancellationToken cancellationToken)
    {
        IQueryable<Resource> query = context.Resources
            .AsNoTracking()
            .Include(r => r.Course)
            .Include(r => r.PostedBy);

        if (!seesEverything)
        {
            // Materialised into a list so this becomes a plain IN clause rather than anything
            // clever. A post with no course is platform wide and always survives the filter.
            List<Guid> allowed = [.. visibleCourseIds];

            query = query.Where(r => r.CourseId == null || allowed.Contains(r.CourseId.Value));
        }

        if (courseId is { } wanted)
        {
            query = query.Where(r => r.CourseId == wanted);
        }

        if (kind is { } wantedKind)
        {
            query = query.Where(r => r.Kind == wantedKind);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            string needle = $"%{search.Trim()}%";

            query = query.Where(r =>
                EF.Functions.ILike(r.Title, needle)
                || (r.Description != null && EF.Functions.ILike(r.Description, needle)));
        }

        return await query
            .OrderByDescending(r => r.CreatedAtUtc)
            .Take(200)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> VisibleCourseIdsAsync(
        Guid userId, CancellationToken cancellationToken)
    {
        // Courses they are studying, plus courses they teach. A dropped enrolment stops the posts
        // for that course appearing, which is the same rule the catalogue applies.
        List<Guid> enrolled = await context.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == userId && e.Status != EnrollmentStatus.Dropped)
            .Select(e => e.CourseId)
            .ToListAsync(cancellationToken);

        List<Guid> teaching = await context.Courses
            .AsNoTracking()
            .Where(c => c.LecturerId == userId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        return [.. enrolled.Union(teaching)];
    }
}
