using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Reports;

namespace NovaLearn.Persistence.Repositories;

internal sealed class ReportRunRepository(ApplicationDbContext context) : IReportRunRepository
{
    public async Task AddAsync(ReportRun run, CancellationToken cancellationToken) =>
        await context.Set<ReportRun>().AddAsync(run, cancellationToken);

    public async Task<IReadOnlyList<ReportRun>> ListRecentAsync(int count, CancellationToken cancellationToken) =>
        await context.Set<ReportRun>()
            .AsNoTracking()
            .Include(r => r.GeneratedBy)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Take(count)
            .ToListAsync(cancellationToken);
}
