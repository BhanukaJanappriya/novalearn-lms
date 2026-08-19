using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Payments.Common;
using NovaLearn.Domain.Payments;
using NovaLearn.Shared.Common;

namespace NovaLearn.Persistence.Repositories;

/// <summary>EF Core implementation of the payment port.</summary>
internal sealed class PaymentRepository(ApplicationDbContext context) : IPaymentRepository
{
    public async Task AddAsync(Payment payment, CancellationToken cancellationToken) =>
        await context.Payments.AddAsync(payment, cancellationToken);

    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Payments
            .Include(p => p.Student)
            .Include(p => p.Course)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Payment?> GetByCheckoutSessionIdAsync(
        string checkoutSessionId, CancellationToken cancellationToken) =>
        context.Payments
            .FirstOrDefaultAsync(p => p.ProviderCheckoutSessionId == checkoutSessionId, cancellationToken);

    public Task<bool> IsWebhookEventProcessedAsync(
        string providerEventId, CancellationToken cancellationToken) =>
        context.ProcessedWebhookEvents
            .AsNoTracking()
            .AnyAsync(e => e.ProviderEventId == providerEventId, cancellationToken);

    public async Task MarkWebhookEventProcessedAsync(
        string providerEventId, DateTimeOffset processedAtUtc, CancellationToken cancellationToken) =>
        await context.ProcessedWebhookEvents.AddAsync(
            ProcessedWebhookEvent.Create(providerEventId, processedAtUtc), cancellationToken);

    public async Task<PagedResult<TransactionDto>> ListTransactionsAsync(
        PaymentStatus? status,
        string? search,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<Payment> query = context.Payments
            .AsNoTracking()
            .Include(p => p.Student)
            .Include(p => p.Course);

        if (status is { } wantedStatus)
        {
            query = query.Where(p => p.Status == wantedStatus);
        }

        if (fromUtc is { } from)
        {
            query = query.Where(p => p.CreatedAtUtc >= from);
        }

        if (toUtc is { } to)
        {
            query = query.Where(p => p.CreatedAtUtc < to);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            string needle = $"%{search.Trim()}%";

            query = query.Where(p =>
                EF.Functions.ILike(p.CourseTitle, needle)
                || (p.Student != null && EF.Functions.ILike(p.Student.Email!, needle))
                || (p.Student != null && EF.Functions.ILike(p.Student.FirstName + " " + p.Student.LastName, needle)));
        }

        query = query.OrderByDescending(p => p.CreatedAtUtc);

        int totalCount = await query.CountAsync(cancellationToken);

        List<Payment> pageItems = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TransactionDto>(
            pageItems.Select(PaymentMapper.ToTransactionDto).ToList(), page, pageSize, totalCount);
    }
}
