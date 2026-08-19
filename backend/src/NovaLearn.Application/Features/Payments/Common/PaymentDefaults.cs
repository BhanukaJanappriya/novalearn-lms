namespace NovaLearn.Application.Features.Payments.Common;

/// <summary>
/// Product-level constants for payments. The platform prices and charges in one currency for now
/// — there is no per-course or per-region currency choice anywhere in the domain — so this is a
/// single constant rather than configuration.
/// </summary>
public static class PaymentDefaults
{
    /// <summary>ISO 4217 code, lower case (Stripe's own convention).</summary>
    public const string Currency = "usd";
}
