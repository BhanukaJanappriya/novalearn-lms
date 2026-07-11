using NovaLearn.Application.Common.Models;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>
/// Read-side port for admin analytics. Implemented in Persistence with EF Core queries,
/// keeping the DbContext out of the Application layer.
/// </summary>
public interface IAdminStatisticsService
{
    Task<AdminStatistics> GetStatisticsAsync(CancellationToken cancellationToken);
}
