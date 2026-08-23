namespace NovaLearn.Application.Features.Reports.Common;

/// <summary>
/// The page size used when a report reuses an existing paged repository method in place of a true
/// unlimited query — large enough that no report on this platform's actual data scale is ever cut
/// short, without the query ever being allowed to run genuinely unbounded.
/// </summary>
public static class ReportExport
{
    public const int MaxRows = 5000;
}

/// <summary>Renders the filters a report was run with into the short summary stored on its <c>ReportRun</c>.</summary>
public static class ReportFilters
{
    public static string? Summarize(params (string Key, object? Value)[] filters)
    {
        List<string> parts = filters
            .Where(f => f.Value is not null)
            .Select(f => $"{f.Key}={f.Value}")
            .ToList();

        return parts.Count == 0 ? null : string.Join(", ", parts);
    }
}
