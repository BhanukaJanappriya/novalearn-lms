namespace NovaLearn.Infrastructure.Payments;

/// <summary>
/// Stripe credentials. Bound from the "Stripe" configuration section, which is deliberately empty
/// in every committed appsettings file — these are meant to come from user-secrets locally
/// (<c>dotnet user-secrets set "Stripe:SecretKey" "sk_test_..."</c>) or environment variables in
/// any real deployment, never from a file that gets committed, test-mode or not.
/// </summary>
public sealed class StripeOptions
{
    public const string SectionName = "Stripe";

    /// <summary>The account's secret key (sk_test_... / sk_live_...). Server side only.</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>The signing secret for this endpoint's webhook (whsec_...), from the Stripe CLI or the dashboard.</summary>
    public string WebhookSecret { get; set; } = string.Empty;
}
