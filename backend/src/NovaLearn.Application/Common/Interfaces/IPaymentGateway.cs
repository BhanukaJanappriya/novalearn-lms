namespace NovaLearn.Application.Common.Interfaces;

/// <summary>What starting a checkout needs to tell the gateway.</summary>
public sealed record CheckoutSessionRequest(
    string CourseTitle,
    decimal Amount,
    string Currency,
    string SuccessUrl,
    string CancelUrl,
    IReadOnlyDictionary<string, string> Metadata);

/// <summary>Where the gateway put the checkout, and its id for later correlation.</summary>
public sealed record CheckoutSessionResult(string SessionId, string CheckoutUrl);

/// <summary>
/// A webhook event, reduced to the handful of fields any handler here needs. Nothing from the
/// gateway's own SDK types crosses this boundary, so the use cases stay ignorant of which
/// provider is behind it.
/// </summary>
public sealed record GatewayWebhookEvent(
    string EventId,
    string EventType,
    string? CheckoutSessionId,
    string? PaymentIntentId);

/// <summary>The result of a refund the gateway has already carried out.</summary>
public sealed record RefundResult(string RefundId, decimal AmountRefunded);

/// <summary>
/// Port to the payment provider. Implemented over Stripe in Infrastructure; the Application layer
/// never sees a Stripe type.
/// </summary>
public interface IPaymentGateway
{
    Task<CheckoutSessionResult> CreateCheckoutSessionAsync(
        CheckoutSessionRequest request, CancellationToken cancellationToken);

    Task<RefundResult> RefundAsync(
        string paymentIntentId, decimal amount, string currency, CancellationToken cancellationToken);

    /// <summary>
    /// Verifies the payload was genuinely sent by the gateway and not forged, then extracts what a
    /// handler needs. Throws when the signature does not check out — this is the one gate standing
    /// between the internet and code that creates enrolments and moves the ledger.
    /// </summary>
    GatewayWebhookEvent ParseWebhookEvent(string payload, string signatureHeader);
}
