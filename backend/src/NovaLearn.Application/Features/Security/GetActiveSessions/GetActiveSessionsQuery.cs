using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Security.Common;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Security.GetActiveSessions;

/// <summary>Every currently active session, paged and optionally filtered by account. Staff only.</summary>
public sealed record GetActiveSessionsQuery(string? Search, int Page, int PageSize)
    : IRequest<Result<PagedResult<SessionRow>>>;

public sealed class GetActiveSessionsQueryHandler(ISecurityRepository security, ICurrentUser currentUser)
    : IRequestHandler<GetActiveSessionsQuery, Result<PagedResult<SessionRow>>>
{
    public async Task<Result<PagedResult<SessionRow>>> Handle(
        GetActiveSessionsQuery request, CancellationToken cancellationToken)
    {
        if (!SecurityAuthority.IsStaff(currentUser))
        {
            return Result.Failure<PagedResult<SessionRow>>(SecurityErrors.StaffOnly);
        }

        return Result.Success(await security.ListActiveSessionsAsync(
            request.Search, request.Page, request.PageSize, cancellationToken));
    }
}
