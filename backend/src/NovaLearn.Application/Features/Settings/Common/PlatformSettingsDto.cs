using NovaLearn.Domain.Settings;

namespace NovaLearn.Application.Features.Settings.Common;

/// <summary>Platform settings as the admin screen sees them.</summary>
public sealed record PlatformSettingsDto(
    string SiteName,
    string SupportEmail,
    bool AllowNewRegistrations,
    bool MaintenanceModeEnabled,
    string? MaintenanceMessage,
    string DefaultCurrency,
    int MaxUploadSizeMb,
    DateTimeOffset? UpdatedAtUtc,
    string? UpdatedBy);

/// <summary>
/// What an anonymous visitor may see: branding, whether the platform is presently in maintenance,
/// and whether it is accepting new accounts — someone about to fill in the registration form
/// needs to know that before submitting it, not from the 403 afterwards. What stays out is the
/// business tuning nobody browsing the site needs: a currency or an upload ceiling is nobody's
/// business but the admins who set it.
/// </summary>
public sealed record PublicSettingsDto(
    string SiteName,
    string SupportEmail,
    bool AllowNewRegistrations,
    bool MaintenanceModeEnabled,
    string? MaintenanceMessage);

public static class PlatformSettingsMapper
{
    public static PlatformSettingsDto ToDto(PlatformSettings settings) =>
        new(
            settings.SiteName,
            settings.SupportEmail,
            settings.AllowNewRegistrations,
            settings.MaintenanceModeEnabled,
            settings.MaintenanceMessage,
            settings.DefaultCurrency,
            settings.MaxUploadSizeMb,
            settings.UpdatedAtUtc,
            settings.UpdatedBy);
}
