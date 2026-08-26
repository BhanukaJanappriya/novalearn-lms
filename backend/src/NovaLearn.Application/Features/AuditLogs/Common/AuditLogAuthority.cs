using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Application.Features.AuditLogs.Common;

/// <summary>
/// Who may read the audit trail. Mirrors <c>ReportAuthority</c>/<c>PaymentAuthority</c> — the
/// controller already restricts this route by role, and this exists anyway so the rule is
/// testable without the HTTP pipeline and cannot drift from what the route attribute says.
/// </summary>
public static class AuditLogAuthority
{
    public static bool IsStaff(ICurrentUser currentUser) =>
        currentUser.IsInRole(Roles.Administrator) || currentUser.IsInRole(Roles.SuperAdministrator);
}
