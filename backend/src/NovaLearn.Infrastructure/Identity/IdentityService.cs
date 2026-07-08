using Microsoft.AspNetCore.Identity;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.Infrastructure.Identity;

/// <summary>
/// ASP.NET Identity implementation of <see cref="IIdentityService"/>. Translates Identity's
/// result/lockout mechanics into the Application's <see cref="Result"/> vocabulary and keeps
/// credential errors generic to prevent account enumeration.
/// </summary>
public sealed class IdentityService(UserManager<ApplicationUser> userManager, IDateTimeProvider dateTimeProvider)
    : IIdentityService
{
    public async Task<Result<AuthenticatedUser>> CreateUserAsync(
        string email, string password, string firstName, string lastName, CancellationToken cancellationToken)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return Result.Failure<AuthenticatedUser>(AuthenticationErrors.EmailAlreadyInUse);
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = false,
            IsActive = true
        };

        IdentityResult created = await userManager.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            return Result.Failure<AuthenticatedUser>(MapCreationError(created));
        }

        await userManager.AddToRoleAsync(user, Roles.Default);

        return await BuildAuthenticatedUserAsync(user);
    }

    public async Task<Result<AuthenticatedUser>> ValidateCredentialsAsync(
        string email, string password, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result.Failure<AuthenticatedUser>(AuthenticationErrors.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            return Result.Failure<AuthenticatedUser>(AuthenticationErrors.AccountInactive);
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return Result.Failure<AuthenticatedUser>(AuthenticationErrors.AccountLockedOut);
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user);
            return Result.Failure<AuthenticatedUser>(AuthenticationErrors.InvalidCredentials);
        }

        if (!user.EmailConfirmed)
        {
            return Result.Failure<AuthenticatedUser>(AuthenticationErrors.EmailNotConfirmed);
        }

        await userManager.ResetAccessFailedCountAsync(user);
        return await BuildAuthenticatedUserAsync(user);
    }

    public async Task<Result<AuthenticatedUser>> FindByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || !user.IsActive)
        {
            return Result.Failure<AuthenticatedUser>(AuthenticationErrors.InvalidCredentials);
        }

        return await BuildAuthenticatedUserAsync(user);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        ApplicationUser user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException($"User {userId} not found.");

        return await userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<Result> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(AuthenticationErrors.InvalidEmailConfirmationToken);
        }

        IdentityResult result = await userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(AuthenticationErrors.InvalidEmailConfirmationToken);
    }

    public async Task RecordSuccessfulLoginAsync(Guid userId, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return;
        }

        user.LastLoginAtUtc = dateTimeProvider.UtcNow;
        await userManager.UpdateAsync(user);
    }

    private async Task<AuthenticatedUser> BuildAuthenticatedUserAsync(ApplicationUser user)
    {
        IList<string> roles = await userManager.GetRolesAsync(user);
        return new AuthenticatedUser(
            user.Id, user.Email!, user.FirstName, user.LastName, user.EmailConfirmed, roles.ToList());
    }

    private static Error MapCreationError(IdentityResult result)
    {
        if (result.Errors.Any(e => e.Code is "DuplicateUserName" or "DuplicateEmail"))
        {
            return AuthenticationErrors.EmailAlreadyInUse;
        }

        string description = string.Join(" ", result.Errors.Select(e => e.Description));
        return AuthenticationErrors.Identity(description);
    }
}
