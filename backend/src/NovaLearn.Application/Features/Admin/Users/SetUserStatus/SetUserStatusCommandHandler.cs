using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Admin.Users.Common;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Admin.Users.SetUserStatus;

public sealed class SetUserStatusCommandHandler(
    IUserDirectory directory,
    IUserAdministration users,
    ICurrentUser currentUser)
    : IRequestHandler<SetUserStatusCommand, Result<AdminUserDto>>
{
    public async Task<Result<AdminUserDto>> Handle(
        SetUserStatusCommand request, CancellationToken cancellationToken)
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

        // Deactivating a super administrator is as good as removing them, so it faces the same
        // last-one-standing check as demotion.
        if (!request.IsActive)
        {
            int superAdmins = await directory.CountInRoleAsync(Roles.SuperAdministrator, cancellationToken);
            if (UserAdminPolicy.CheckNotStrandingPlatform(target, retainsSuperAdmin: false, superAdmins) is { } stranded)
            {
                return Result.Failure<AdminUserDto>(stranded);
            }
        }

        Result result = await users.SetActiveAsync(request.UserId, request.IsActive, cancellationToken);
        if (result.IsFailure)
        {
            return Result.Failure<AdminUserDto>(result.Error);
        }

        return await ReloadAsync(request.UserId, cancellationToken);
    }

    private async Task<Result<AdminUserDto>> ReloadAsync(Guid userId, CancellationToken cancellationToken)
    {
        AdminUserRow? updated = await directory.GetAsync(userId, cancellationToken);
        return updated is null
            ? Result.Failure<AdminUserDto>(UserAdminErrors.NotFound)
            : AdminUserDto.FromRow(updated);
    }
}
