using NovaLearn.Domain.Common;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Domain.Reports;

/// <summary>
/// An audit record of one report having been generated: who ran it, what it was, the filters they
/// used, and how many rows it returned. Logged as a side effect of every report query so staff can
/// see who has been pulling what data, without that being the report's own job to track.
/// </summary>
public sealed class ReportRun : BaseEntity
{
    private ReportRun() { } // EF Core

    public ReportType Type { get; private set; }

    public Guid GeneratedById { get; private set; }

    /// <summary>Optional navigation to the staff member who ran it (for read projections).</summary>
    public ApplicationUser? GeneratedBy { get; private set; }

    /// <summary>A short human-readable rendering of the filters used, e.g. "status=Active, from=2026-01-01".</summary>
    public string? FiltersSummary { get; private set; }

    public int RowCount { get; private set; }

    public static ReportRun Create(
        ReportType type, Guid generatedById, string? filtersSummary, int rowCount, DateTimeOffset now)
    {
        return new ReportRun
        {
            Type = type,
            GeneratedById = generatedById,
            FiltersSummary = string.IsNullOrWhiteSpace(filtersSummary) ? null : filtersSummary.Trim(),
            RowCount = rowCount,
            CreatedAtUtc = now
        };
    }
}
