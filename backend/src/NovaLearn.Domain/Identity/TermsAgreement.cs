namespace NovaLearn.Domain.Identity;

/// <summary>
/// The Terms of Service and Privacy Policy that new accounts are currently asked to accept.
///
/// <see cref="CurrentVersion"/> is an ISO date. Bump it whenever the published wording changes
/// materially, so the version each account accepted stays on record against
/// <see cref="ApplicationUser.TermsVersion"/> and existing users can be asked to re-accept.
/// </summary>
public static class TermsAgreement
{
    public const string CurrentVersion = "2026-09-07";
}
