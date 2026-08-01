using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Admin.Users.Common;

/// <summary>
/// The guardrails around account administration, in one place so every use case enforces the
/// same rules. Each check returns the offending <see cref="Error"/>, or null when allowed.
///
/// The endpoints are already restricted to administrators; these rules constrain what an
/// administrator may do to <em>whom</em>, which is where the real risk sits: locking yourself
/// out, escalating your own privileges, or stranding the platform with no super administrator.
/// </summary>
public static class UserAdminPolicy
{
    /// <summary>Whether the caller holds the super administrator role.</summary>
    public static bool IsSuperAdmin(ICurrentUser currentUser) =>
        currentUser.IsInRole(Roles.SuperAdministrator);

    /// <summary>
    /// Common gate for acting on another account: you may not act on yourself, and only a
    /// super administrator may touch a super administrator.
    /// </summary>
    public static Error? CheckCanModify(AdminUserRow target, ICurrentUser currentUser)
    {
        if (currentUser.UserId is not { } callerId)
        {
            return UserAdminErrors.Unauthenticated;
        }

        // Self-service through the admin console is how administrators lock themselves out.
        if (target.Id == callerId)
        {
            return UserAdminErrors.CannotModifySelf;
        }

        if (target.Roles.Contains(Roles.SuperAdministrator) && !IsSuperAdmin(currentUser))
        {
            return UserAdminErrors.SuperAdminOnly;
        }

        return null;
    }

    /// <summary>
    /// Validates a requested role set: every name must be real, and only a super administrator
    /// may add or remove the super administrator role.
    /// </summary>
    public static Error? CheckRoleAssignment(
        AdminUserRow target, IReadOnlyList<string> requestedRoles, ICurrentUser currentUser)
    {
        foreach (string role in requestedRoles)
        {
            if (!Roles.All.Contains(role))
            {
                return UserAdminErrors.UnknownRole(role);
            }
        }

        bool hadSuperAdmin = target.Roles.Contains(Roles.SuperAdministrator);
        bool wantsSuperAdmin = requestedRoles.Contains(Roles.SuperAdministrator);

        // Privilege escalation guard: an ordinary administrator cannot mint a peer above them,
        // nor strip the role from someone who has it.
        if (hadSuperAdmin != wantsSuperAdmin && !IsSuperAdmin(currentUser))
        {
            return UserAdminErrors.CannotGrantSuperAdmin;
        }

        return null;
    }

    /// <summary>
    /// Stops the platform being stranded with no usable super administrator, whether by
    /// demoting the last one or by deactivating them.
    /// </summary>
    public static Error? CheckNotStrandingPlatform(
        AdminUserRow target, bool retainsSuperAdmin, int superAdminCount)
    {
        bool isSuperAdmin = target.Roles.Contains(Roles.SuperAdministrator);

        if (isSuperAdmin && !retainsSuperAdmin && superAdminCount <= 1)
        {
            return UserAdminErrors.LastSuperAdmin;
        }

        return null;
    }
}
