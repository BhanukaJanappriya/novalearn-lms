using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Admin.Users.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Admin.Users.VerifyUserEmail;

public sealed class VerifyUserEmailCommandHandler(
    IUserDirectory directory,
    IUserAdministration users,
    ICurrentUser currentUser)
    : IRequestHandler<VerifyUserEmailCommand, Result<AdminUserDto>>
{
    public async Task<Result<AdminUserDto>> Handle(
        VerifyUserEmailCommand request, CancellationToken cancellationToken)
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

        Result confirmed = await users.ConfirmEmailManuallyAsync(request.UserId, cancellationToken);
        if (confirmed.IsFailure)
        {
            return Result.Failure<AdminUserDto>(confirmed.Error);
        }

        // A stuck registration is often also locked out from retrying, so clear both together.
        Result unlocked = await users.ClearLockoutAsync(request.UserId, cancellationToken);
        if (unlocked.IsFailure)
        {
            return Result.Failure<AdminUserDto>(unlocked.Error);
        }

        AdminUserRow? updated = await directory.GetAsync(request.UserId, cancellationToken);
        return updated is null
            ? Result.Failure<AdminUserDto>(UserAdminErrors.NotFound)
            : AdminUserDto.FromRow(updated);
    }
}
