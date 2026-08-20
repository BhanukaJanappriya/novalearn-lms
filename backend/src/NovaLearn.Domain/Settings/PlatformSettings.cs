using NovaLearn.Domain.Common;

namespace NovaLearn.Domain.Settings;

/// <summary>
/// Platform-wide configuration an administrator can change without a redeploy: branding, whether
/// the platform is accepting new accounts, maintenance mode, and the two business limits
/// (currency, upload size) that used to be fixed in code.
///
/// A singleton by construction rather than by convention: it always lives at <see cref="SingletonId"/>,
/// so every reader agrees on which row is the real one without a lookup of its own, and there is
/// no "which row wins" question if two callers ever raced to seed it.
/// </summary>
public sealed class PlatformSettings : BaseEntity
{
    public static readonly Guid SingletonId = new("00000000-0000-0000-0000-000000000001");

    private PlatformSettings() { } // EF Core

    public string SiteName { get; private set; } = null!;

    public string SupportEmail { get; private set; } = null!;

    /// <summary>Whether the public registration form accepts new accounts.</summary>
    public bool AllowNewRegistrations { get; private set; }

    public bool MaintenanceModeEnabled { get; private set; }

    /// <summary>Shown to a blocked caller while maintenance mode is on. Optional.</summary>
    public string? MaintenanceMessage { get; private set; }

    /// <summary>ISO 4217 code, lower case — what a new checkout charges in.</summary>
    public string DefaultCurrency { get; private set; } = null!;

    public int MaxUploadSizeMb { get; private set; }

    /// <summary>The row seeding creates once, the first time the platform starts.</summary>
    public static PlatformSettings CreateDefault() =>
        new()
        {
            Id = SingletonId,
            SiteName = "NovaLearn",
            SupportEmail = "support@novalearn.local",
            AllowNewRegistrations = true,
            MaintenanceModeEnabled = false,
            MaintenanceMessage = null,
            DefaultCurrency = "usd",
            MaxUploadSizeMb = 200
        };

    /// <summary>
    /// Applies an edit. Callers are expected to have already validated shape (a plausible email,
    /// a three letter currency code); this is the last-line guard against the state genuinely
    /// being unusable, not the first line of user-facing feedback.
    /// </summary>
    public void Update(
        string siteName,
        string supportEmail,
        bool allowNewRegistrations,
        bool maintenanceModeEnabled,
        string? maintenanceMessage,
        string defaultCurrency,
        int maxUploadSizeMb)
    {
        if (string.IsNullOrWhiteSpace(siteName))
        {
            throw new ArgumentException("A site name is required.", nameof(siteName));
        }

        if (string.IsNullOrWhiteSpace(supportEmail))
        {
            throw new ArgumentException("A support email is required.", nameof(supportEmail));
        }

        if (defaultCurrency is not { Length: 3 })
        {
            throw new ArgumentException("Currency must be a three letter ISO code.", nameof(defaultCurrency));
        }

        if (maxUploadSizeMb is < 1 or > 500)
        {
            throw new ArgumentException("Upload size must be between 1 and 500 MB.", nameof(maxUploadSizeMb));
        }

        SiteName = siteName.Trim();
        SupportEmail = supportEmail.Trim();
        AllowNewRegistrations = allowNewRegistrations;
        MaintenanceModeEnabled = maintenanceModeEnabled;
        MaintenanceMessage = string.IsNullOrWhiteSpace(maintenanceMessage) ? null : maintenanceMessage.Trim();
        DefaultCurrency = defaultCurrency.Trim().ToLowerInvariant();
        MaxUploadSizeMb = maxUploadSizeMb;
    }
}
