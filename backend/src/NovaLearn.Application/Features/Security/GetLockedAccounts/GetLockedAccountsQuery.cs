using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Security.Common;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Security.GetLockedAccounts;

/// <summary>Every account currently locked out by repeated failed sign-ins. Staff only.</summary>
public sealed record GetLockedAccountsQuery(string? Search, int Page, int PageSize)
    : IRequest<Result<PagedResult<LockedAccountRow>>>;

public sealed class GetLockedAccountsQueryHandler(ISecurityRepository security, ICurrentUser currentUser)
    : IRequestHandler<GetLockedAccountsQuery, Result<PagedResult<LockedAccountRow>>>
{
    public async Task<Result<PagedResult<LockedAccountRow>>> Handle(
        GetLockedAccountsQuery request, CancellationToken cancellationToken)
    {
        if (!SecurityAuthority.IsStaff(currentUser))
        {
            return Result.Failure<PagedResult<LockedAccountRow>>(SecurityErrors.StaffOnly);
        }

        return Result.Success(await security.ListLockedAccountsAsync(
            request.Search, request.Page, request.PageSize, cancellationToken));
    }
}
