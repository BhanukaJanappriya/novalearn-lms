using MediatR;
using NovaLearn.Application.Features.Admin.Users.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Admin.Users.UpdateUserRoles;

/// <summary>Replaces an account's roles with exactly the set supplied.</summary>
public sealed record UpdateUserRolesCommand(Guid UserId, IReadOnlyList<string> Roles)
    : IRequest<Result<AdminUserDto>>;
