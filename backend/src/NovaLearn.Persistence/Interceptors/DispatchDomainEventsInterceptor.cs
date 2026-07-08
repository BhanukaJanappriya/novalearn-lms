using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NovaLearn.Domain.Common;

namespace NovaLearn.Persistence.Interceptors;

/// <summary>
/// Publishes domain events raised by aggregates via MediatR after changes are saved, so handlers
/// observe a committed state. Events are collected and cleared before dispatch to avoid re-entry.
/// </summary>
public sealed class DispatchDomainEventsInterceptor(IPublisher publisher) : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await DispatchAsync(eventData.Context, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchAsync(DbContext context, CancellationToken cancellationToken)
    {
        List<IHasDomainEvents> aggregates = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        List<IDomainEvent> events = aggregates.SelectMany(a => a.DomainEvents).ToList();

        aggregates.ForEach(a => a.ClearDomainEvents());

        foreach (IDomainEvent domainEvent in events)
        {
            await publisher.Publish(domainEvent, cancellationToken);
        }
    }
}
