using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Admin.Users.Common;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Admin.Users.UpdateUserRoles;

public sealed class UpdateUserRolesCommandHandler(
    IUserDirectory directory,
    IUserAdministration users,
    ICurrentUser currentUser)
    : IRequestHandler<UpdateUserRolesCommand, Result<AdminUserDto>>
{
    public async Task<Result<AdminUserDto>> Handle(
        UpdateUserRolesCommand request, CancellationToken cancellationToken)
    {
        AdminUserRow? target = await directory.GetAsync(request.UserId, cancellationToken);
        if (target is null)
        {
            return Result.Failure<AdminUserDto>(UserAdminErrors.NotFound);
        }

        if (UserAdminPolicy.CheckCanModify(target, currentUser) is { } denied)
        {
            return Result.Failure<AdminUserDto>(denied);
        }

        if (UserAdminPolicy.CheckRoleAssignment(target, request.Roles, currentUser) is { } rejected)
        {
            return Result.Failure<AdminUserDto>(rejected);
        }

        bool retainsSuperAdmin = request.Roles.Contains(Roles.SuperAdministrator);
        if (!retainsSuperAdmin)
        {
            int superAdmins = await directory.CountInRoleAsync(Roles.SuperAdministrator, cancellationToken);
            if (UserAdminPolicy.CheckNotStrandingPlatform(target, retainsSuperAdmin, superAdmins) is { } stranded)
            {
                return Result.Failure<AdminUserDto>(stranded);
            }
        }

        Result result = await users.SetRolesAsync(request.UserId, request.Roles, cancellationToken);
        if (result.IsFailure)
        {
            return Result.Failure<AdminUserDto>(result.Error);
        }

        AdminUserRow? updated = await directory.GetAsync(request.UserId, cancellationToken);
        return updated is null
            ? Result.Failure<AdminUserDto>(UserAdminErrors.NotFound)
            : AdminUserDto.FromRow(updated);
    }
}
