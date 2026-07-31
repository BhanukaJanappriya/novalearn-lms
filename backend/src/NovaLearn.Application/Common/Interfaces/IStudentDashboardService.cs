using NovaLearn.Application.Common.Models;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>
/// Read-side port for the student dashboard. Mirrors <see cref="IAdminStatisticsService"/>: the
/// projection spans several aggregates, so it is expressed as a query service rather than being
/// forced onto one repository.
/// </summary>
public interface IStudentDashboardService
{
    Task<StudentStatistics> GetForStudentAsync(Guid studentId, CancellationToken cancellationToken);
}
