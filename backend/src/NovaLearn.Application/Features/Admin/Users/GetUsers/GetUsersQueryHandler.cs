using MediatR;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Admin.Users.Common;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Admin.Users.GetUsers;

public sealed class GetUsersQueryHandler(IUserDirectory directory)
    : IRequestHandler<GetUsersQuery, Result<PagedResult<AdminUserDto>>>
{
    public async Task<Result<PagedResult<AdminUserDto>>> Handle(
        GetUsersQuery request, CancellationToken cancellationToken)
    {
        PagedResult<AdminUserRow> page = await directory.SearchAsync(
            request.Search,
            request.Role,
            request.IsActive,
            request.EmailConfirmed,
            request.Page,
            request.PageSize,
            cancellationToken);

        return new PagedResult<AdminUserDto>(
            page.Items.Select(AdminUserDto.FromRow).ToList(),
            page.Page,
            page.PageSize,
            page.TotalCount);
    }
}
