using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Application.Features.Support.Common;

/// <summary>
/// Who handles support tickets. Deliberately narrower than most other admin-facing slices: a
/// lecturer or teaching assistant submits tickets like any other user, they do not triage them —
/// this is administrator territory only, the same way finance is.
/// </summary>
public static class SupportAuthority
{
    public static bool IsStaff(ICurrentUser currentUser) =>
        currentUser.IsInRole(Roles.Administrator) || currentUser.IsInRole(Roles.SuperAdministrator);
}
