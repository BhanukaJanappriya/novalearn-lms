namespace NovaLearn.Shared.Common;

/// <summary>
/// A single page of results plus the paging metadata a client needs to render controls.
/// <see cref="TotalPages"/> is derived, so callers never have to recompute it.
/// </summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    /// <summary>Total number of pages for <see cref="TotalCount"/> at <see cref="PageSize"/>.</summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;

    /// <summary>An empty page, useful as a neutral result.</summary>
    public static PagedResult<T> Empty(int page, int pageSize) => new([], page, pageSize, 0);
}
