using NovaLearn.Domain.Common;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Domain.Notifications;

/// <summary>
/// One item in a person's notification feed. Deliberately denormalised: the title, message and
/// link are captured when the notification is raised, so the feed still reads correctly after
/// the thing it refers to is renamed or deleted.
/// </summary>
public sealed class Notification : BaseEntity
{
    private Notification() { } // EF Core

    /// <summary>Who the notification is for.</summary>
    public Guid RecipientId { get; private set; }

    public NotificationType Type { get; private set; }

    public string Title { get; private set; } = null!;

    public string Message { get; private set; } = null!;

    /// <summary>Client-side route to open, e.g. <c>/my-courses/{id}/assignments</c>.</summary>
    public string? Link { get; private set; }

    public bool IsRead { get; private set; }

    public DateTimeOffset? ReadAtUtc { get; private set; }

    public ApplicationUser? Recipient { get; private set; }

    public static Notification Create(
        Guid recipientId,
        NotificationType type,
        string title,
        string message,
        string? link) =>
        new()
        {
            RecipientId = recipientId,
            Type = type,
            Title = title.Trim(),
            Message = message.Trim(),
            Link = string.IsNullOrWhiteSpace(link) ? null : link.Trim(),
            IsRead = false
        };

    /// <summary>Marks it read. Idempotent, so the first read time is never overwritten.</summary>
    public void MarkRead(DateTimeOffset readAtUtc)
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        ReadAtUtc = readAtUtc;
    }
}
