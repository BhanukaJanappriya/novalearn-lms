using NovaLearn.Domain.Common;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Support.Events;

namespace NovaLearn.Domain.Support;

/// <summary>
/// A support request and its thread, from the first report through to close. The first message
/// is not a separate "description" field: it is simply the first item in <see cref="Messages"/>,
/// so a ticket's report and every reply to it live in one ordered place rather than two.
/// </summary>
public sealed class SupportTicket : BaseEntity
{
    public const int SubjectMaxLength = 200;

    private readonly List<SupportTicketMessage> _messages = [];

    private SupportTicket() { } // EF Core

    public Guid SubmittedById { get; private set; }

    public string Subject { get; private set; } = null!;

    public TicketCategory Category { get; private set; }

    public TicketPriority Priority { get; private set; }

    public TicketStatus Status { get; private set; }

    /// <summary>The staff member currently handling this ticket. Null while unclaimed.</summary>
    public Guid? AssignedToId { get; private set; }

    public DateTimeOffset? ResolvedAtUtc { get; private set; }

    public DateTimeOffset? ClosedAtUtc { get; private set; }

    public IReadOnlyCollection<SupportTicketMessage> Messages => _messages.AsReadOnly();

    public ApplicationUser? SubmittedBy { get; private set; }

    public ApplicationUser? AssignedTo { get; private set; }

    /// <summary>Everything staff need at a glance without opening the thread.</summary>
    public int MessageCount => _messages.Count;

    public DateTimeOffset LastActivityAtUtc =>
        _messages.Count == 0 ? CreatedAtUtc : _messages.Max(m => m.CreatedAtUtc);

    public static SupportTicket Create(
        Guid submittedById,
        string subject,
        TicketCategory category,
        TicketPriority priority,
        string firstMessage,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("A ticket needs a subject.", nameof(subject));
        }

        if (string.IsNullOrWhiteSpace(firstMessage))
        {
            throw new ArgumentException("A ticket needs a description.", nameof(firstMessage));
        }

        var ticket = new SupportTicket
        {
            SubmittedById = submittedById,
            Subject = subject.Trim(),
            Category = category,
            Priority = priority,
            Status = TicketStatus.Open,
            CreatedAtUtc = now
        };

        ticket._messages.Add(
            SupportTicketMessage.Create(ticket.Id, submittedById, firstMessage, isInternalNote: false, now));

        return ticket;
    }

    /// <summary>
    /// Adds a message to the thread and returns it.
    ///
    /// The caller needs that returned message for one reason only: to hand it to the repository's
    /// own explicit add. <see cref="BaseEntity"/> assigns its key client-side, so a new message
    /// reached only through this already-tracked ticket's collection saves as a no-op update
    /// instead of an insert — the same rule <c>AttemptAnswer</c> is added under, for the same
    /// reason, and the one this method's return value exists to make impossible to skip.
    ///
    /// The first reply from staff moves an open ticket into progress; the submitter replying to a
    /// resolved or closed ticket reopens it, on the reasoning that a reply to a settled ticket
    /// almost always means it was not actually settled. Neither happens for an internal note,
    /// which is staff talking to each other about the ticket rather than to the person who raised it.
    /// </summary>
    public SupportTicketMessage Reply(Guid authorId, string body, bool isInternalNote, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("A reply needs a body.", nameof(body));
        }

        bool authorIsSubmitter = authorId == SubmittedById;

        if (authorIsSubmitter && isInternalNote)
        {
            throw new InvalidOperationException("The person who raised a ticket cannot leave an internal note.");
        }

        SupportTicketMessage message = SupportTicketMessage.Create(Id, authorId, body, isInternalNote, now);
        _messages.Add(message);

        if (!isInternalNote)
        {
            if (!authorIsSubmitter && Status == TicketStatus.Open)
            {
                SetStatus(TicketStatus.InProgress, now);
            }
            else if (authorIsSubmitter && Status is TicketStatus.Resolved or TicketStatus.Closed)
            {
                SetStatus(TicketStatus.Open, now);
            }

            Guid? recipientId = authorIsSubmitter ? AssignedToId : SubmittedById;

            RaiseDomainEvent(
                new SupportTicketRepliedDomainEvent(Id, Subject, authorId, SubmittedById, recipientId));
        }

        return message;
    }

    /// <summary>Claims the ticket for a staff member, or hands it to someone else.</summary>
    public void AssignTo(Guid adminId) => AssignedToId = adminId;

    /// <summary>Returns the ticket to the unclaimed queue.</summary>
    public void Unassign() => AssignedToId = null;

    public void ChangePriority(TicketPriority priority) => Priority = priority;

    public void ChangeStatus(TicketStatus status, DateTimeOffset now)
    {
        if (Status == status)
        {
            return;
        }

        SetStatus(status, now);
        RaiseDomainEvent(new SupportTicketStatusChangedDomainEvent(Id, Subject, SubmittedById, status));
    }

    private void SetStatus(TicketStatus status, DateTimeOffset now)
    {
        Status = status;
        ResolvedAtUtc = status == TicketStatus.Resolved ? now : null;
        ClosedAtUtc = status == TicketStatus.Closed ? now : null;
    }
}
