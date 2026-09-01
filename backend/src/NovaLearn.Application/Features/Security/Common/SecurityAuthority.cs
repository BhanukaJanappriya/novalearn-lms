using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Application.Features.Security.Common;

/// <summary>
/// Who may view and act on the security center. Mirrors <c>ReportAuthority</c>/<c>AuditLogAuthority</c>
/// — the controller already restricts these routes by role, and this exists anyway so the rule is
/// testable without the HTTP pipeline and cannot drift from what the route attribute says.
/// </summary>
public static class SecurityAuthority
{
    public static bool IsStaff(ICurrentUser currentUser) =>
        currentUser.IsInRole(Roles.Administrator) || currentUser.IsInRole(Roles.SuperAdministrator);
}
