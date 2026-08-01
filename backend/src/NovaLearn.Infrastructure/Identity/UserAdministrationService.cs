using Microsoft.AspNetCore.Identity;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.Infrastructure.Identity;

/// <summary>
/// ASP.NET Identity adapter for account administration. Authority rules live in the Application
/// layer's <c>UserAdminPolicy</c>; this type only carries out decisions already made.
/// </summary>
internal sealed class UserAdministrationService(UserManager<ApplicationUser> userManager)
    : IUserAdministration
{
    public async Task<Result> SetActiveAsync(Guid userId, bool isActive, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(UserAdminErrors.NotFound);
        }

        user.IsActive = isActive;

        // Bump the security stamp so any live session or refresh token for a deactivated
        // account stops being honoured, rather than only blocking the next sign-in.
        await userManager.UpdateSecurityStampAsync(user);

        return ToResult(await userManager.UpdateAsync(user));
    }

    public async Task<Result> SetRolesAsync(
        Guid userId, IReadOnlyList<string> roles, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(UserAdminErrors.NotFound);
        }

        IList<string> current = await userManager.GetRolesAsync(user);

        string[] toRemove = current.Except(roles, StringComparer.Ordinal).ToArray();
        string[] toAdd = roles.Except(current, StringComparer.Ordinal).ToArray();

        if (toRemove.Length > 0)
        {
            IdentityResult removed = await userManager.RemoveFromRolesAsync(user, toRemove);
            if (!removed.Succeeded)
            {
                return ToResult(removed);
            }
        }

        if (toAdd.Length > 0)
        {
            IdentityResult added = await userManager.AddToRolesAsync(user, toAdd);
            if (!added.Succeeded)
            {
                return ToResult(added);
            }
        }

        // Roles are baked into the JWT, so an existing token would keep the old set until it
        // expired. Invalidating the stamp forces a fresh one.
        await userManager.UpdateSecurityStampAsync(user);

        return Result.Success();
    }

    public async Task<Result> ConfirmEmailManuallyAsync(Guid userId, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(UserAdminErrors.NotFound);
        }

        if (user.EmailConfirmed)
        {
            return Result.Success();
        }

        user.EmailConfirmed = true;
        return ToResult(await userManager.UpdateAsync(user));
    }

    public async Task<Result> ClearLockoutAsync(Guid userId, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(UserAdminErrors.NotFound);
        }

        IdentityResult reset = await userManager.ResetAccessFailedCountAsync(user);
        if (!reset.Succeeded)
        {
            return ToResult(reset);
        }

        return ToResult(await userManager.SetLockoutEndDateAsync(user, null));
    }

    private static Result ToResult(IdentityResult result) =>
        result.Succeeded
            ? Result.Success()
            : Result.Failure(UserAdminErrors.Identity(
                string.Join("; ", result.Errors.Select(e => e.Description))));
}
