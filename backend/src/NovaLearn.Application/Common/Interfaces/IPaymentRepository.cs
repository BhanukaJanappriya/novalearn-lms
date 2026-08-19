using NovaLearn.Application.Features.Payments.Common;
using NovaLearn.Domain.Payments;
using NovaLearn.Shared.Common;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>Persistence port for the <see cref="Payment"/> aggregate.</summary>
public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken);

    /// <summary>Loads a payment with its student and course, or null.</summary>
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Looks a payment up by Stripe's Checkout Session id — how a webhook finds it.</summary>
    Task<Payment?> GetByCheckoutSessionIdAsync(
        string checkoutSessionId, CancellationToken cancellationToken);

    /// <summary>Whether this Stripe event id has already been handled.</summary>
    Task<bool> IsWebhookEventProcessedAsync(string providerEventId, CancellationToken cancellationToken);

    /// <summary>Records a Stripe event id as handled, so a redelivery becomes a no-op.</summary>
    Task MarkWebhookEventProcessedAsync(
        string providerEventId, DateTimeOffset processedAtUtc, CancellationToken cancellationToken);

    /// <summary>The admin ledger: every payment, newest first, with optional status/search/date filters.</summary>
    Task<PagedResult<TransactionDto>> ListTransactionsAsync(
        PaymentStatus? status,
        string? search,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
