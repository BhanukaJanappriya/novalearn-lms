using Microsoft.AspNetCore.Identity;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Profile.Common;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.Infrastructure.Identity;

/// <summary>ASP.NET Identity adapter for a person editing their own profile.</summary>
internal sealed class ProfileService(UserManager<ApplicationUser> userManager) : IProfileService
{
    public async Task<MyProfileDto?> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return null;
        }

        IList<string> roles = await userManager.GetRolesAsync(user);

        return new MyProfileDto(
            user.Id,
            user.FullName,
            user.FirstName,
            user.LastName,
            user.Email ?? string.Empty,
            user.AvatarUrl,
            [.. roles],
            user.CreatedAtUtc);
    }

    public async Task<Result> SetAvatarAsync(
        Guid userId, string? avatarUrl, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(ProfileErrors.NotFound);
        }

        user.AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();

        IdentityResult result = await userManager.UpdateAsync(user);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(
                Error.Validation(
                    "profile.update_failed",
                    string.Join("; ", result.Errors.Select(e => e.Description))));
    }
}
