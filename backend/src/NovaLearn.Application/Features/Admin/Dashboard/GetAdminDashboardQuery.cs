using MediatR;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Admin.Dashboard;

/// <summary>
/// Builds the admin dashboard aggregate from live platform statistics.
/// </summary>
/// <param name="Days">
/// Length of the window the enrollment/completion trend charts cover, counting back from now.
/// Everything else in the payload (KPIs, feeds, health) is unaffected by this — only the two
/// trend charts are windowed, the same reasoning platform analytics uses for its own series.
/// </param>
public sealed record GetAdminDashboardQuery(int Days) : IRequest<Result<AdminDashboardResponse>>;
