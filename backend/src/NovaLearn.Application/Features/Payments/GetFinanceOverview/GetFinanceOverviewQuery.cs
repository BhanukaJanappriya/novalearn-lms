using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Payments.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Payments.GetFinanceOverview;

/// <summary>The finance page's headline figures, revenue trend and course breakdown for a window.</summary>
public sealed record GetFinanceOverviewQuery(int Days) : IRequest<Result<FinanceOverview>>;

/// <summary>
/// Serves the finance page. Same window clamp as platform analytics — a range picker can only ever
/// send the values it offers, so an out-of-range request is answered with the nearest sensible
/// window rather than an error.
/// </summary>
public sealed class GetFinanceOverviewQueryHandler(IFinanceOverview overview, ICurrentUser currentUser)
    : IRequestHandler<GetFinanceOverviewQuery, Result<FinanceOverview>>
{
    private const int MinimumDays = 7;
    private const int MaximumDays = 365;

    public async Task<Result<FinanceOverview>> Handle(
        GetFinanceOverviewQuery request, CancellationToken cancellationToken)
    {
        if (!PaymentAuthority.IsAdmin(currentUser))
        {
            return Result.Failure<FinanceOverview>(PaymentErrors.Forbidden);
        }

        int days = Math.Clamp(request.Days, MinimumDays, MaximumDays);

        return Result.Success(await overview.GetAsync(days, cancellationToken));
    }
}
