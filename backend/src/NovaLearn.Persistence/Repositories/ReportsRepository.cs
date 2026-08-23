using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Reports.Common;
using NovaLearn.Domain.Enrollments;

namespace NovaLearn.Persistence.Repositories;

/// <summary>EF Core implementation of the one report query with no existing read model to reuse.</summary>
internal sealed class ReportsRepository(ApplicationDbContext context) : IReportsRepository
{
    public async Task<IReadOnlyList<EnrollmentReportRow>> ListEnrollmentsAsync(
        EnrollmentStatus? status,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        CancellationToken cancellationToken)
    {
        IQueryable<Enrollment> query = context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course);

        if (status is { } wantedStatus)
        {
            query = query.Where(e => e.Status == wantedStatus);
        }

        if (fromUtc is { } from)
        {
            query = query.Where(e => e.EnrolledAtUtc >= from);
        }

        if (toUtc is { } to)
        {
            query = query.Where(e => e.EnrolledAtUtc < to);
        }

        List<Enrollment> rows = await query
            .OrderByDescending(e => e.EnrolledAtUtc)
            .Take(ReportExport.MaxRows)
            .ToListAsync(cancellationToken);

        return rows
            .Select(e => new EnrollmentReportRow(
                e.Id,
                e.StudentId,
                e.Student is { } student ? $"{student.FirstName} {student.LastName}".Trim() : "Unknown",
                e.Student?.Email ?? string.Empty,
                e.CourseId,
                e.Course?.Title ?? "Unknown",
                e.Status,
                e.ProgressPercent,
                e.EnrolledAtUtc,
                e.CompletedAtUtc))
            .ToList();
    }
}
