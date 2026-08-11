using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the people directory.
///
/// Every aggregate is a separate flat query stitched together in memory rather than a
/// correlated subquery inside the projection. Two earlier read models in this codebase failed at
/// runtime for exactly that reason, and a handful of small queries over a directory-sized result
/// is cheaper than being clever.
/// </summary>
internal sealed class PeopleDirectory(ApplicationDbContext context) : IPeopleDirectory
{
    public Task<IReadOnlyList<DirectoryEntry>> ListStudentsAsync(
        string? search, CancellationToken cancellationToken) =>
        BuildAsync([Roles.Student], search, includeLearnerStats: true, cancellationToken);

    public Task<IReadOnlyList<DirectoryEntry>> ListTeachingStaffAsync(
        string? search, CancellationToken cancellationToken) =>
        BuildAsync(
            [Roles.Lecturer, Roles.TeachingAssistant], search, includeLearnerStats: false, cancellationToken);

    private async Task<IReadOnlyList<DirectoryEntry>> BuildAsync(
        string[] wantedRoles,
        string? search,
        bool includeLearnerStats,
        CancellationToken cancellationToken)
    {
        // Materialised first so the filter below becomes a plain parameterised IN clause.
        List<Guid> memberIds = await (
                from userRole in context.UserRoles
                join role in context.Roles on userRole.RoleId equals role.Id
                where wantedRoles.Contains(role.Name!)
                select userRole.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (memberIds.Count == 0)
        {
            return [];
        }

        IQueryable<ApplicationUser> query = context.Users.Where(u => memberIds.Contains(u.Id));

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search.Trim()}%";
            query = query.Where(u =>
                EF.Functions.ILike(u.FirstName, pattern)
                || EF.Functions.ILike(u.LastName, pattern)
                || EF.Functions.ILike(u.Email!, pattern));
        }

        var people = await query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                Email = u.Email ?? string.Empty,
                u.AvatarUrl,
                u.IsActive,
                u.CreatedAtUtc,
                u.LastLoginAtUtc,
            })
            .ToListAsync(cancellationToken);

        if (people.Count == 0)
        {
            return [];
        }

        List<Guid> pageIds = people.Select(p => p.Id).ToList();

        IReadOnlyDictionary<Guid, List<string>> roles = await LoadRolesAsync(pageIds, cancellationToken);

        Dictionary<Guid, DirectoryLearnerStats> learnerStats = includeLearnerStats
            ? await LoadLearnerStatsAsync(pageIds, cancellationToken)
            : [];

        Dictionary<Guid, DirectoryTeacherStats> teacherStats = includeLearnerStats
            ? []
            : await LoadTeacherStatsAsync(pageIds, cancellationToken);

        return people
            .Select(p => new DirectoryEntry(
                p.Id,
                p.FirstName,
                p.LastName,
                p.Email,
                p.AvatarUrl,
                p.IsActive,
                p.CreatedAtUtc,
                p.LastLoginAtUtc,
                roles.TryGetValue(p.Id, out List<string>? held) ? held : [],
                learnerStats.TryGetValue(p.Id, out DirectoryLearnerStats? learner) ? learner : null,
                teacherStats.TryGetValue(p.Id, out DirectoryTeacherStats? teacher) ? teacher : null))
            .ToList();
    }

    private async Task<IReadOnlyDictionary<Guid, List<string>>> LoadRolesAsync(
        IReadOnlyList<Guid> userIds, CancellationToken cancellationToken)
    {
        var pairs = await (
                from userRole in context.UserRoles
                join role in context.Roles on userRole.RoleId equals role.Id
                where userIds.Contains(userRole.UserId)
                select new { userRole.UserId, role.Name })
            .ToListAsync(cancellationToken);

        return pairs
            .GroupBy(p => p.UserId)
            .ToDictionary(g => g.Key, g => g.Select(p => p.Name ?? string.Empty).Order().ToList());
    }

    private async Task<Dictionary<Guid, DirectoryLearnerStats>> LoadLearnerStatsAsync(
        IReadOnlyList<Guid> studentIds, CancellationToken cancellationToken)
    {
        var rows = await context.Enrollments
            .Where(e => studentIds.Contains(e.StudentId) && e.Status != EnrollmentStatus.Dropped)
            .GroupBy(e => e.StudentId)
            .Select(g => new
            {
                StudentId = g.Key,
                Enrolled = g.Count(),
                Completed = g.Count(e => e.Status == EnrollmentStatus.Completed),
                AverageProgress = g.Average(e => (double?)e.ProgressPercent),
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(
            r => r.StudentId,
            r => new DirectoryLearnerStats(
                r.Enrolled,
                r.Completed,
                (int)Math.Round(r.AverageProgress ?? 0)));
    }

    private async Task<Dictionary<Guid, DirectoryTeacherStats>> LoadTeacherStatsAsync(
        IReadOnlyList<Guid> lecturerIds, CancellationToken cancellationToken)
    {
        var courseRows = await context.Courses
            .Where(c => lecturerIds.Contains(c.LecturerId))
            .GroupBy(c => c.LecturerId)
            .Select(g => new
            {
                LecturerId = g.Key,
                Owned = g.Count(),
                Published = g.Count(c => c.Status == CourseStatus.Published),
            })
            .ToListAsync(cancellationToken);

        // Who teaches which course, so learners can be counted per lecturer without a subquery.
        var ownership = await context.Courses
            .Where(c => lecturerIds.Contains(c.LecturerId))
            .Select(c => new { c.Id, c.LecturerId })
            .ToListAsync(cancellationToken);

        List<Guid> courseIds = ownership.Select(o => o.Id).ToList();

        var enrolments = courseIds.Count == 0
            ? []
            : await context.Enrollments
                .Where(e => courseIds.Contains(e.CourseId) && e.Status != EnrollmentStatus.Dropped)
                .Select(e => new { e.CourseId, e.StudentId })
                .ToListAsync(cancellationToken);

        Dictionary<Guid, Guid> lecturerByCourse = ownership.ToDictionary(o => o.Id, o => o.LecturerId);

        // Distinct per lecturer: one learner on three of their courses is still one learner.
        Dictionary<Guid, HashSet<Guid>> learnersByLecturer = [];
        foreach (var enrolment in enrolments)
        {
            if (!lecturerByCourse.TryGetValue(enrolment.CourseId, out Guid lecturerId))
            {
                continue;
            }

            if (!learnersByLecturer.TryGetValue(lecturerId, out HashSet<Guid>? learners))
            {
                learners = [];
                learnersByLecturer[lecturerId] = learners;
            }

            learners.Add(enrolment.StudentId);
        }

        var headships = await context.Departments
            .Where(d => d.HeadId != null && lecturerIds.Contains(d.HeadId!.Value))
            .Select(d => new { HeadId = d.HeadId!.Value, d.Name })
            .ToListAsync(cancellationToken);

        Dictionary<Guid, List<string>> departmentsByHead = headships
            .GroupBy(h => h.HeadId)
            .ToDictionary(g => g.Key, g => g.Select(h => h.Name).Order().ToList());

        return lecturerIds.ToDictionary(
            id => id,
            id =>
            {
                var courses = courseRows.FirstOrDefault(c => c.LecturerId == id);

                return new DirectoryTeacherStats(
                    courses?.Owned ?? 0,
                    courses?.Published ?? 0,
                    learnersByLecturer.TryGetValue(id, out HashSet<Guid>? learners) ? learners.Count : 0,
                    departmentsByHead.TryGetValue(id, out List<string>? departments) ? departments : []);
            });
    }
}
