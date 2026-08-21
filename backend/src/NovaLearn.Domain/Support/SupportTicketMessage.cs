using NovaLearn.Domain.Common;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Domain.Support;

/// <summary>
/// One message in a ticket's thread — the original report and every reply are the same shape.
/// Append-only: there is no edit or delete, so a thread is always a true record of what was said
/// and by whom, in order.
/// </summary>
public sealed class SupportTicketMessage : BaseEntity
{
    public const int BodyMaxLength = 4000;

    private SupportTicketMessage() { } // EF Core

    public Guid TicketId { get; private set; }

    public Guid AuthorId { get; private set; }

    public string Body { get; private set; } = null!;

    /// <summary>
    /// Visible to staff only. Used for handoff context and internal discussion — never sent to,
    /// or shown to, the person who raised the ticket.
    /// </summary>
    public bool IsInternalNote { get; private set; }

    public ApplicationUser? Author { get; private set; }

    /// <summary>
    /// <paramref name="now"/> is stamped onto <see cref="BaseEntity.CreatedAtUtc"/> directly rather
    /// than left to the save-time interceptor, the same choice <c>RefreshToken</c> makes: the
    /// aggregate's own <c>LastActivityAtUtc</c> needs a real value the moment a message is added,
    /// not only after the next save. The interceptor still runs and confirms it at save time.
    /// </summary>
    internal static SupportTicketMessage Create(
        Guid ticketId, Guid authorId, string body, bool isInternalNote, DateTimeOffset now) =>
        new()
        {
            TicketId = ticketId,
            AuthorId = authorId,
            Body = body.Trim(),
            IsInternalNote = isInternalNote,
            CreatedAtUtc = now
        };
}
