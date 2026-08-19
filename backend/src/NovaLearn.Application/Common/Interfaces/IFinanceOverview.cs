using NovaLearn.Application.Common.Models;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>Read-side port for the finance page's headline figures, trend and course breakdown.</summary>
public interface IFinanceOverview
{
    Task<FinanceOverview> GetAsync(int days, CancellationToken cancellationToken);
}
