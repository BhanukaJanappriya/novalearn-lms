using MediatR;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Admin.Analytics;

/// <summary>
/// Platform analytics for a window of the given length.
/// </summary>
public sealed record GetPlatformAnalyticsQuery(int Days) : IRequest<Result<PlatformAnalytics>>;

/// <summary>
/// Serves the analytics page.
///
/// The window is clamped rather than validated away. A range picker can only send the values it
/// offers, so anything else is a hand written request, and answering it with the nearest sensible
/// window is friendlier than an error and stops a caller asking for ten years of buckets.
/// </summary>
public sealed class GetPlatformAnalyticsQueryHandler(IPlatformAnalytics analytics)
    : IRequestHandler<GetPlatformAnalyticsQuery, Result<PlatformAnalytics>>
{
    private const int MinimumDays = 7;
    private const int MaximumDays = 365;

    public async Task<Result<PlatformAnalytics>> Handle(
        GetPlatformAnalyticsQuery request, CancellationToken cancellationToken)
    {
        int days = Math.Clamp(request.Days, MinimumDays, MaximumDays);

        return Result.Success(await analytics.GetAsync(days, cancellationToken));
    }
}
