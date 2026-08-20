using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Domain.Payments;

namespace NovaLearn.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the finance read model.
///
/// Every figure here is net of refunds, attributed to the day a payment was <em>paid</em> rather
/// than the day it was later refunded on. That means a day's bar can move down on a later refresh
/// if something sold that day gets refunded afterwards — a deliberate choice: this reports each
/// cohort of sales' current standing, not a frozen record of the day itself. A true ledger would
/// post the refund on its own day instead; that is a heavier model than a single admin's revenue
/// dashboard currently needs.
///
/// A related simplification: a payment refunded more than once only remembers the most recent
/// refund's date, not a full history of each partial refund. Multiple partial refunds on one
/// payment are rare enough here that attributing the whole cumulative refund to the last one's
/// date is an acceptable approximation rather than a dedicated refund ledger table.
/// </summary>
internal sealed class FinanceOverviewService(ApplicationDbContext context, ISettingsProvider settings)
    : IFinanceOverview
{
    public async Task<FinanceOverview> GetAsync(int days, CancellationToken cancellationToken)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset from = now.AddDays(-days);
        DateTimeOffset previousFrom = from.AddDays(-days);

        AnalyticsGranularity granularity = AnalyticsBucketing.GranularityFor(days);
        var window = new AnalyticsWindow(from, now, days, granularity);

        FinanceHeadline headline = await BuildHeadlineAsync(from, previousFrom, cancellationToken);
        IReadOnlyList<RevenuePoint> series = await BuildSeriesAsync(from, granularity, cancellationToken);
        IReadOnlyList<CourseRevenueRow> courses = await BuildCoursesAsync(cancellationToken);

        // The currency label on the report tracks what a new checkout charges in right now.
        // Individual rows still carry whatever currency they were actually charged in at the
        // time, which does not change retroactively just because the setting since moved on.
        PlatformSettingsSnapshot platform = await settings.GetAsync(cancellationToken);

        return new FinanceOverview(window, headline, series, courses, platform.DefaultCurrency);
    }

    private async Task<FinanceHeadline> BuildHeadlineAsync(
        DateTimeOffset from, DateTimeOffset previousFrom, CancellationToken cancellationToken)
    {
        (decimal revenue, int count) = await NetRevenueAsync(from, DateTimeOffset.UtcNow, cancellationToken);
        (decimal previousRevenue, int previousCount) =
            await NetRevenueAsync(previousFrom, from, cancellationToken);

        decimal refunded = await context.Payments
            .AsNoTracking()
            .Where(p => p.RefundedAtUtc != null && p.RefundedAtUtc >= from)
            .SumAsync(p => p.RefundedAmount ?? 0, cancellationToken);

        decimal previousRefunded = await context.Payments
            .AsNoTracking()
            .Where(p => p.RefundedAtUtc != null && p.RefundedAtUtc >= previousFrom && p.RefundedAtUtc < from)
            .SumAsync(p => p.RefundedAmount ?? 0, cancellationToken);

        int pending = await context.Payments
            .AsNoTracking()
            .CountAsync(p => p.Status == PaymentStatus.Pending, cancellationToken);

        return new FinanceHeadline(
            new TrendMetric((double)revenue, (double)previousRevenue),
            new TrendMetric(count, previousCount),
            new TrendMetric((double)refunded, (double)previousRefunded),
            pending);
    }

    /// <summary>Net revenue and how many payments contributed to it, for payments paid inside a window.</summary>
    private async Task<(decimal Revenue, int Count)> NetRevenueAsync(
        DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        var rows = await context.Payments
            .AsNoTracking()
            .Where(p => p.PaidAtUtc != null && p.PaidAtUtc >= from && p.PaidAtUtc < to)
            .Select(p => new { p.Amount, p.RefundedAmount })
            .ToListAsync(cancellationToken);

        decimal net = rows.Sum(r => r.Amount - (r.RefundedAmount ?? 0));
        return (net, rows.Count);
    }

    private async Task<IReadOnlyList<RevenuePoint>> BuildSeriesAsync(
        DateTimeOffset from, AnalyticsGranularity granularity, CancellationToken cancellationToken)
    {
        var rows = await context.Payments
            .AsNoTracking()
            .Where(p => p.PaidAtUtc != null && p.PaidAtUtc >= from)
            .Select(p => new { PaidAtUtc = p.PaidAtUtc!.Value, Net = p.Amount - (p.RefundedAmount ?? 0) })
            .ToListAsync(cancellationToken);

        var byBucket = rows
            .GroupBy(r => AnalyticsBucketing.StartOfBucket(
                DateOnly.FromDateTime(r.PaidAtUtc.UtcDateTime), granularity))
            .ToDictionary(g => g.Key, g => (Revenue: g.Sum(r => r.Net), Count: g.Count()));

        return AnalyticsBucketing.AllBuckets(from, DateTimeOffset.UtcNow, granularity)
            .Select(bucket =>
            {
                byBucket.TryGetValue(bucket, out var stat);
                return new RevenuePoint(bucket, stat.Revenue, stat.Count);
            })
            .ToList();
    }

    /// <summary>
    /// Lifetime figures per course, busiest first — same reasoning as platform analytics.
    ///
    /// Grouped in memory rather than in the GROUP BY itself: constructing a record instance
    /// straight out of a grouped projection is exactly the shape that has compiled and then
    /// thrown at runtime elsewhere in this codebase, so the raw rows are pulled flat first.
    /// </summary>
    private async Task<IReadOnlyList<CourseRevenueRow>> BuildCoursesAsync(CancellationToken cancellationToken)
    {
        var rows = await context.Payments
            .AsNoTracking()
            .Where(p => p.PaidAtUtc != null)
            .Select(p => new { p.CourseId, p.CourseTitle, p.Amount, p.RefundedAmount })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(p => new { p.CourseId, p.CourseTitle })
            .Select(g => new CourseRevenueRow(
                g.Key.CourseId,
                g.Key.CourseTitle,
                g.Count(),
                g.Sum(p => p.Amount),
                g.Sum(p => p.RefundedAmount ?? 0)))
            .OrderByDescending(row => row.GrossRevenue)
            .ToList();
    }
}
