using NovaLearn.Domain.Support;
using NovaLearn.Shared.Common;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>Persistence port for the <see cref="SupportTicket"/> aggregate.</summary>
public interface ISupportTicketRepository
{
    Task AddAsync(SupportTicket ticket, CancellationToken cancellationToken);

    /// <summary>
    /// Explicitly tracks a new message as an insert. Required whenever a message is added to a
    /// ticket that was already loaded (tracked) rather than one just being created — see the
    /// remarks on <see cref="SupportTicket.Reply"/> for why the aggregate's own collection append
    /// is not enough on its own.
    /// </summary>
    Task AddMessageAsync(SupportTicketMessage message, CancellationToken cancellationToken);

    /// <summary>Loads a ticket with its thread, submitter and assignee, or null.</summary>
    Task<SupportTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Everything a given user submitted, newest activity first.</summary>
    Task<IReadOnlyList<SupportTicket>> ListForUserAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>The staff queue: every ticket, optionally filtered, newest activity first.</summary>
    Task<PagedResult<SupportTicket>> ListForStaffAsync(
        TicketStatus? status,
        TicketCategory? category,
        TicketPriority? priority,
        Guid? assignedToId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>Counts behind the queue's at-a-glance header: open, unassigned, and high or more urgent.</summary>
    Task<(int Open, int Unassigned, int UrgentOrHigh)> GetStaffCountsAsync(CancellationToken cancellationToken);
}
