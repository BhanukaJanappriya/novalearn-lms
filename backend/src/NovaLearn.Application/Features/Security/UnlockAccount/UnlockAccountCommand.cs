using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Security.Common;
using NovaLearn.Domain.Audit;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Security.UnlockAccount;

/// <summary>Clears a lockout imposed by repeated failed sign-ins, letting the account try again immediately. Staff only.</summary>
public sealed record UnlockAccountCommand(Guid UserId) : IRequest<Result>;

public sealed class UnlockAccountCommandHandler(
    IUserAdministration users,
    IUserDirectory directory,
    ICurrentUser currentUser,
    IAuditLogger auditLogger)
    : IRequestHandler<UnlockAccountCommand, Result>
{
    public async Task<Result> Handle(UnlockAccountCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId || !SecurityAuthority.IsStaff(currentUser))
        {
            return Result.Failure(SecurityErrors.StaffOnly);
        }

        var target = await directory.GetAsync(request.UserId, cancellationToken);
        if (target is null)
        {
            return Result.Failure(UserAdminErrors.NotFound);
        }

        Result unlocked = await users.ClearLockoutAsync(request.UserId, cancellationToken);
        if (unlocked.IsFailure)
        {
            return unlocked;
        }

        await auditLogger.RecordAsync(
            callerId,
            AuditCategory.Security,
            "Unlocked account",
            target.FullName,
            "User",
            target.Id,
            cancellationToken);

        return Result.Success();
    }
}
