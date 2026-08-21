using NovaLearn.Domain.Support;

namespace NovaLearn.Application.Features.Support.Common;

/// <summary>
/// One message in a ticket thread, as a viewer allowed to see it sees it.
///
/// Carries no "is this author staff" flag of its own: a ticket has exactly two kinds of author,
/// the person who submitted it and whoever on staff replies, so comparing <see cref="AuthorId"/>
/// against the ticket's own <c>submittedById</c> already answers that — and an internal note is
/// staff by construction, since the domain refuses to let a submitter create one.
/// </summary>
public sealed record TicketMessageDto(
    Guid Id,
    Guid AuthorId,
    string AuthorName,
    string? AuthorAvatarUrl,
    string Body,
    bool IsInternalNote,
    DateTimeOffset CreatedAtUtc);

/// <summary>A ticket with its full thread — internal notes included only for a staff viewer.</summary>
public sealed record TicketDetailDto(
    Guid Id,
    string Subject,
    TicketCategory Category,
    TicketPriority Priority,
    TicketStatus Status,
    Guid SubmittedById,
    string SubmittedByName,
    string SubmittedByEmail,
    Guid? AssignedToId,
    string? AssignedToName,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ResolvedAtUtc,
    DateTimeOffset? ClosedAtUtc,
    IReadOnlyList<TicketMessageDto> Messages);

/// <summary>One row in a ticket list — the submitter's own list or the staff queue.</summary>
public sealed record TicketSummaryDto(
    Guid Id,
    string Subject,
    TicketCategory Category,
    TicketPriority Priority,
    TicketStatus Status,
    Guid SubmittedById,
    string SubmittedByName,
    Guid? AssignedToId,
    string? AssignedToName,
    int MessageCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset LastActivityAtUtc);

public static class SupportTicketMapper
{
    public static TicketDetailDto ToDetailDto(SupportTicket ticket, bool includeInternalNotes)
    {
        IEnumerable<SupportTicketMessage> messages = ticket.Messages
            .Where(m => includeInternalNotes || !m.IsInternalNote)
            .OrderBy(m => m.CreatedAtUtc);

        return new TicketDetailDto(
            ticket.Id,
            ticket.Subject,
            ticket.Category,
            ticket.Priority,
            ticket.Status,
            ticket.SubmittedById,
            FullName(ticket.SubmittedBy),
            ticket.SubmittedBy?.Email ?? string.Empty,
            ticket.AssignedToId,
            ticket.AssignedTo is null ? null : FullName(ticket.AssignedTo),
            ticket.CreatedAtUtc,
            ticket.ResolvedAtUtc,
            ticket.ClosedAtUtc,
            messages.Select(ToMessageDto).ToList());
    }

    public static TicketSummaryDto ToSummaryDto(SupportTicket ticket) =>
        new(
            ticket.Id,
            ticket.Subject,
            ticket.Category,
            ticket.Priority,
            ticket.Status,
            ticket.SubmittedById,
            FullName(ticket.SubmittedBy),
            ticket.AssignedToId,
            ticket.AssignedTo is null ? null : FullName(ticket.AssignedTo),
            ticket.MessageCount,
            ticket.CreatedAtUtc,
            ticket.LastActivityAtUtc);

    private static TicketMessageDto ToMessageDto(SupportTicketMessage message) =>
        new(
            message.Id,
            message.AuthorId,
            FullName(message.Author),
            message.Author?.AvatarUrl,
            message.Body,
            message.IsInternalNote,
            message.CreatedAtUtc);

    private static string FullName(Domain.Identity.ApplicationUser? user) =>
        user is null ? "Unknown" : $"{user.FirstName} {user.LastName}".Trim();
}
