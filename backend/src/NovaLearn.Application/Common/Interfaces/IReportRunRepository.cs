using NovaLearn.Domain.Reports;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>The audit trail of report generation.</summary>
public interface IReportRunRepository
{
    Task AddAsync(ReportRun run, CancellationToken cancellationToken);

    /// <summary>The most recent runs across every report type, newest first.</summary>
    Task<IReadOnlyList<ReportRun>> ListRecentAsync(int count, CancellationToken cancellationToken);
}
