using NovaLearn.Application.Common.Models;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>
/// Read-side port for platform analytics.
///
/// Separate from <see cref="IAdminStatisticsService"/>, which answers "what is the state of the
/// platform right now" for the dashboard. This answers "how is it going", which needs a window, a
/// comparison against the window before it, and figures about learning rather than about accounts.
/// </summary>
public interface IPlatformAnalytics
{
    /// <param name="days">Length of the window, counting back from now.</param>
    Task<PlatformAnalytics> GetAsync(int days, CancellationToken cancellationToken);
}
