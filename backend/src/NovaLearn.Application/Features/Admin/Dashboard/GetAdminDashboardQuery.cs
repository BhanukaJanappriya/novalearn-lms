using MediatR;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Admin.Dashboard;

/// <summary>Builds the admin dashboard aggregate from live platform statistics.</summary>
public sealed record GetAdminDashboardQuery : IRequest<Result<AdminDashboardResponse>>;
