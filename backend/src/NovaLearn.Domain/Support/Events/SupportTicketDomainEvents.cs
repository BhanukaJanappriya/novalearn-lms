using NovaLearn.Domain.Common;

namespace NovaLearn.Domain.Support.Events;

// Creating a ticket raises no event of its own. There is no single recipient for "a new ticket
// exists" the way there is for a reply or a status change — every administrator is not one
// recipient but a broadcast this codebase has no mechanism for, and would be noisy at any real
// volume besides. Staff find new tickets through the queue's own open and unassigned counts
// instead of being pushed one notification per ticket.

/// <summary>
/// Raised on a reply that is not an internal note, so the other side of the conversation is told.
/// <paramref name="RecipientId"/> is null when staff reply to a ticket nobody has claimed yet —
/// nobody specific to notify, rather than every administrator on the platform.
///
/// Carries <paramref name="SubmittedById"/> so its handler can pick the right link (the
/// submitter's own ticket page, or the staff queue's) without querying the aggregate again.
/// Domain events are dispatched from inside the interceptor that runs right after the triggering
/// save commits, on the same DbContext; re-querying an entity that context is still holding
/// tracked, mid-dispatch, is what caused a spurious concurrency exception on this exact event
/// during testing, not a hypothetical risk.
/// </summary>
public sealed record SupportTicketRepliedDomainEvent(
    Guid TicketId,
    string Subject,
    Guid AuthorId,
    Guid SubmittedById,
    Guid? RecipientId) : DomainEvent;

/// <summary>Raised when staff change a ticket's status, so the submitter is told where things stand.</summary>
public sealed record SupportTicketStatusChangedDomainEvent(
    Guid TicketId,
    string Subject,
    Guid SubmittedById,
    TicketStatus Status) : DomainEvent;
