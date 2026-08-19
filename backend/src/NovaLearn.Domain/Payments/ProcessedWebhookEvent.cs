namespace NovaLearn.Domain.Payments;

/// <summary>
/// A record that a given Stripe webhook event has already been handled.
///
/// Stripe's delivery guarantee is at-least-once, so the same event id can arrive more than once.
/// <see cref="Payment"/>'s own state transitions are written to tolerate a repeat, but a refund
/// additionally calls out to Stripe, and that call must not fire twice for one event. This table
/// is checked before any side effect runs, keyed on Stripe's own event id, deliberately separate
/// from <see cref="BaseEntity"/>: it is a pure idempotency ledger with nothing to audit or soft
/// delete, just a fact and when it was learned.
/// </summary>
public sealed class ProcessedWebhookEvent
{
    private ProcessedWebhookEvent() { } // EF Core

    /// <summary>Stripe's event id (e.g. "evt_..."). The primary key: existence is the whole point.</summary>
    public string ProviderEventId { get; private set; } = null!;

    public DateTimeOffset ProcessedAtUtc { get; private set; }

    public static ProcessedWebhookEvent Create(string providerEventId, DateTimeOffset processedAtUtc) =>
        new()
        {
            ProviderEventId = providerEventId,
            ProcessedAtUtc = processedAtUtc
        };
}
