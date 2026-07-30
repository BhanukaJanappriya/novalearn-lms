using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Enrollments.Common;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Shared.Common;

namespace NovaLearn.Persistence.Repositories;

public sealed class EnrollmentRepository(ApplicationDbContext dbContext) : IEnrollmentRepository
{
    public async Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken) =>
        await dbContext.Enrollments.AddAsync(enrollment, cancellationToken);

    public Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public void Remove(Enrollment enrollment) => dbContext.Enrollments.Remove(enrollment);

    public Task<Enrollment?> GetActiveAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken) =>
        dbContext.Enrollments
            .FirstOrDefaultAsync(
                e => e.StudentId == studentId
                    && e.CourseId == courseId
                    && e.Status != EnrollmentStatus.Dropped,
                cancellationToken);

    public async Task<IReadOnlyList<Enrollment>> ListForStudentAsync(
        Guid studentId, CancellationToken cancellationToken) =>
        await dbContext.Enrollments
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.EnrolledAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Enrollment>> ListForCourseAsync(
        Guid courseId, CancellationToken cancellationToken) =>
        await dbContext.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Where(e => e.CourseId == courseId)
            .OrderByDescending(e => e.EnrolledAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<PagedResult<CourseCatalogDto>> GetCatalogAsync(
        string? search,
        string? category,
        CourseLevel? level,
        Guid? studentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        int safePage = page < 1 ? 1 : page;
        int safePageSize = pageSize < 1 ? 12 : pageSize;

        // Guid.Empty never matches a real user, so an unauthenticated caller simply sees
        // IsEnrolled = false everywhere without needing a second query shape.
        Guid viewerId = studentId ?? Guid.Empty;

        IQueryable<Course> query = dbContext.Courses.Where(c => c.Status == CourseStatus.Published);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search.Trim()}%";
            query = query.Where(c =>
                EF.Functions.ILike(c.Title, pattern) || EF.Functions.ILike(c.Code, pattern));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(c => c.Category == category);
        }

        if (level.HasValue)
        {
            query = query.Where(c => c.Level == level.Value);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        // Project to an anonymous shape first: enum-to-string mapping happens in memory so no
        // provider-specific translation is required.
        var rows = await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Code,
                c.Description,
                c.Category,
                c.Level,
                c.Price,
                c.CoverImageUrl,
                c.LecturerId,
                LecturerFirstName = c.Lecturer == null ? null : c.Lecturer.FirstName,
                LecturerLastName = c.Lecturer == null ? null : c.Lecturer.LastName,
                c.CreatedAtUtc,
                EnrolledCount = dbContext.Enrollments
                    .Count(e => e.CourseId == c.Id && e.Status != EnrollmentStatus.Dropped),
                IsEnrolled = dbContext.Enrollments
                    .Any(e => e.CourseId == c.Id
                        && e.StudentId == viewerId
                        && e.Status != EnrollmentStatus.Dropped)
            })
            .ToListAsync(cancellationToken);

        List<CourseCatalogDto> items = rows
            .Select(r => new CourseCatalogDto(
                r.Id,
                r.Title,
                r.Code,
                r.Description,
                r.Category,
                r.Level.ToString(),
                r.Price,
                r.CoverImageUrl,
                r.LecturerId,
                $"{r.LecturerFirstName} {r.LecturerLastName}".Trim() is { Length: > 0 } name ? name : "Unknown",
                r.EnrolledCount,
                r.IsEnrolled,
                r.CreatedAtUtc))
            .ToList();

        return new PagedResult<CourseCatalogDto>(items, safePage, safePageSize, totalCount);
    }
}
