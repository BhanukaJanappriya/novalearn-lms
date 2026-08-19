using NovaLearn.Application.Common.Models;

namespace NovaLearn.Persistence.Repositories;

/// <summary>
/// Shared date-bucketing for every read model that draws a chart over a window: platform
/// analytics and finance today, potentially others later. Kept in one place because the two
/// already have to agree — a course completions chart and a revenue chart covering the same
/// thirty days must bucket by the same days, or the two dashboards would read as though they
/// disagreed about the calendar.
/// </summary>
internal static class AnalyticsBucketing
{
    /// <summary>How finely to bucket a window of this length.</summary>
    public static AnalyticsGranularity GranularityFor(int days) =>
        days switch
        {
            <= 31 => AnalyticsGranularity.Day,
            <= 120 => AnalyticsGranularity.Week,
            _ => AnalyticsGranularity.Month
        };

    /// <summary>Counts a set of timestamps into buckets of the given granularity.</summary>
    public static Dictionary<DateOnly, int> Count(
        IEnumerable<DateTimeOffset> dates, AnalyticsGranularity granularity) =>
        dates
            .GroupBy(date => StartOfBucket(DateOnly.FromDateTime(date.UtcDateTime), granularity))
            .ToDictionary(group => group.Key, group => group.Count());

    public static DateOnly StartOfBucket(DateOnly date, AnalyticsGranularity granularity) =>
        granularity switch
        {
            AnalyticsGranularity.Day => date,
            AnalyticsGranularity.Week => date.AddDays(-(int)date.DayOfWeek),
            _ => new DateOnly(date.Year, date.Month, 1)
        };

    /// <summary>
    /// Every bucket in the window, including empty ones. A chart that silently omits a quiet
    /// bucket draws a straight line through it and overstates activity.
    /// </summary>
    public static IEnumerable<DateOnly> AllBuckets(
        DateTimeOffset from, DateTimeOffset to, AnalyticsGranularity granularity)
    {
        DateOnly cursor = StartOfBucket(DateOnly.FromDateTime(from.UtcDateTime), granularity);
        DateOnly last = StartOfBucket(DateOnly.FromDateTime(to.UtcDateTime), granularity);

        while (cursor <= last)
        {
            yield return cursor;

            cursor = granularity switch
            {
                AnalyticsGranularity.Day => cursor.AddDays(1),
                AnalyticsGranularity.Week => cursor.AddDays(7),
                _ => cursor.AddMonths(1)
            };
        }
    }
}
