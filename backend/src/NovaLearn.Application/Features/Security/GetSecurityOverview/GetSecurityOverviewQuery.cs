using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Security.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Security.GetSecurityOverview;

/// <summary>The security center's headline figures. Staff only.</summary>
public sealed record GetSecurityOverviewQuery : IRequest<Result<SecurityOverview>>;

public sealed class GetSecurityOverviewQueryHandler(ISecurityRepository security, ICurrentUser currentUser)
    : IRequestHandler<GetSecurityOverviewQuery, Result<SecurityOverview>>
{
    public async Task<Result<SecurityOverview>> Handle(
        GetSecurityOverviewQuery request, CancellationToken cancellationToken)
    {
        if (!SecurityAuthority.IsStaff(currentUser))
        {
            return Result.Failure<SecurityOverview>(SecurityErrors.StaffOnly);
        }

        return Result.Success(await security.GetOverviewAsync(cancellationToken));
    }
}
