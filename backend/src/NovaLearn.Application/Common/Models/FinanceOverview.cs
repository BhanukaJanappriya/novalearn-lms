namespace NovaLearn.Application.Common.Models;

/// <summary>One bucket on the revenue chart.</summary>
public sealed record RevenuePoint(DateOnly Date, decimal Revenue, int Transactions);

/// <summary>How one course has done, across its whole life rather than the window — same reasoning as <c>CoursePerformanceRow</c> in platform analytics.</summary>
public sealed record CourseRevenueRow(
    Guid CourseId,
    string CourseTitle,
    int Transactions,
    decimal GrossRevenue,
    decimal RefundedAmount)
{
    public decimal NetRevenue => GrossRevenue - RefundedAmount;
}

/// <summary>The headline figures at the top of the finance page, each with its previous-window comparison.</summary>
public sealed record FinanceHeadline(
    TrendMetric NetRevenue,
    TrendMetric Transactions,
    TrendMetric RefundedAmount,
    int PendingCheckouts);

/// <summary>Everything the finance page shows, computed for one window.</summary>
public sealed record FinanceOverview(
    AnalyticsWindow Window,
    FinanceHeadline Headline,
    IReadOnlyList<RevenuePoint> Series,
    IReadOnlyList<CourseRevenueRow> Courses,
    string Currency);
