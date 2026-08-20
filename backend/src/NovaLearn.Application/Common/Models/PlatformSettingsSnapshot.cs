namespace NovaLearn.Application.Common.Models;

/// <summary>
/// A read-only, cache-friendly copy of platform settings, for the many places across the codebase
/// that need to consult a setting in passing (can this course be checked out in what currency,
/// is registration open, is the platform in maintenance) without each becoming a database call or
/// depending on the full <c>PlatformSettings</c> aggregate.
/// </summary>
public sealed record PlatformSettingsSnapshot(
    string SiteName,
    string SupportEmail,
    bool AllowNewRegistrations,
    bool MaintenanceModeEnabled,
    string? MaintenanceMessage,
    string DefaultCurrency,
    int MaxUploadSizeMb);
