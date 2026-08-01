using MediatR;
using NovaLearn.Application.Features.Admin.Users.Common;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Admin.Users.GetUsers;

/// <summary>Paged, filtered account search for the admin console. All filters are optional.</summary>
public sealed record GetUsersQuery(
    string? Search,
    string? Role,
    bool? IsActive,
    bool? EmailConfirmed,
    int Page,
    int PageSize) : IRequest<Result<PagedResult<AdminUserDto>>>;
