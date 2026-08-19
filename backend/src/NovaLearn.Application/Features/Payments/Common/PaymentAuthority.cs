using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Application.Features.Payments.Common;

/// <summary>
/// Who may see and act on the finance ledger, in one place. Mirrors <c>ResourceAuthority</c> and
/// <c>AssessmentAuthority</c> from the other admin-facing slices: the controller already restricts
/// these routes by role, and this exists anyway so the rule is testable without the HTTP pipeline
/// and cannot drift from what the route attribute says.
/// </summary>
public static class PaymentAuthority
{
    public static bool IsAdmin(ICurrentUser currentUser) =>
        currentUser.IsInRole(Roles.Administrator) || currentUser.IsInRole(Roles.SuperAdministrator);
}
