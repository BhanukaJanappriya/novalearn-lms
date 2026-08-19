using Microsoft.Extensions.Options;
using NovaLearn.Application.Common.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace NovaLearn.Infrastructure.Payments;

/// <summary>
/// Implements <see cref="IPaymentGateway"/> over Stripe Checkout. This is the only file in the
/// solution allowed to know that Stripe is the provider; everything upstream talks to the port.
/// </summary>
internal sealed class StripePaymentGateway(IOptions<StripeOptions> options) : IPaymentGateway
{
    private readonly StripeOptions _options = options.Value;

    /// <summary>
    /// A fresh client per call rather than one built in the constructor.
    ///
    /// DI activates every constructor dependency before a handler's body runs at all, so a client
    /// built eagerly here would throw the moment anything merely depended on this gateway — even
    /// a request that would fail its own validation before ever reaching Stripe, like checking
    /// out a free course. Building it lazily, at the point a call actually needs it, is what lets
    /// that validation run and answer first, and keeps a missing key surfacing as the use case's
    /// own caught, readable error instead of an unhandled exception thrown during DI activation.
    /// </summary>
    private StripeClient Client() => new(_options.SecretKey);

    public async Task<CheckoutSessionResult> CreateCheckoutSessionAsync(
        CheckoutSessionRequest request, CancellationToken cancellationToken)
    {
        var sessions = new SessionService(Client());

        var sessionOptions = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = request.SuccessUrl,
            CancelUrl = request.CancelUrl,
            Metadata = new Dictionary<string, string>(request.Metadata),
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = request.Currency,
                        UnitAmount = ToMinorUnits(request.Amount),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = request.CourseTitle
                        }
                    }
                }
            ]
        };

        Session session = await sessions.CreateAsync(
            sessionOptions, requestOptions: null, cancellationToken: cancellationToken);

        return new CheckoutSessionResult(session.Id, session.Url);
    }

    public async Task<RefundResult> RefundAsync(
        string paymentIntentId, decimal amount, string currency, CancellationToken cancellationToken)
    {
        var refunds = new RefundService(Client());

        var refundOptions = new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId,
            Amount = ToMinorUnits(amount),
            Currency = currency
        };

        Refund refund = await refunds.CreateAsync(
            refundOptions, requestOptions: null, cancellationToken: cancellationToken);

        return new RefundResult(refund.Id, FromMinorUnits(refund.Amount));
    }

    public GatewayWebhookEvent ParseWebhookEvent(string payload, string signatureHeader)
    {
        // Throws Stripe.StripeException on a bad signature, which the caller (the webhook use
        // case) translates into its own error rather than letting a Stripe type escape this file.
        Event stripeEvent = EventUtility.ConstructEvent(payload, signatureHeader, _options.WebhookSecret);

        Session? session = stripeEvent.Data.Object as Session;

        return new GatewayWebhookEvent(
            stripeEvent.Id, stripeEvent.Type, session?.Id, session?.PaymentIntentId);
    }

    /// <summary>Stripe amounts are integers in the smallest unit of the currency (cents for usd).</summary>
    private static long ToMinorUnits(decimal amount) =>
        (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);

    private static decimal FromMinorUnits(long? minorUnits) => (minorUnits ?? 0) / 100m;
}
