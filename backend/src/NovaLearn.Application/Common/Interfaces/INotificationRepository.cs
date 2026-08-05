using NovaLearn.Domain.Notifications;
using NovaLearn.Shared.Common;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>Persistence port for a person's notification feed.</summary>
public interface INotificationRepository
{
    Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken);

    /// <summary>A person's notifications, newest first.</summary>
    Task<PagedResult<Notification>> ListAsync(
        Guid recipientId, bool unreadOnly, int page, int pageSize, CancellationToken cancellationToken);

    Task<Notification?> GetAsync(Guid notificationId, CancellationToken cancellationToken);

    /// <summary>How many are still unread, for the badge.</summary>
    Task<int> CountUnreadAsync(Guid recipientId, CancellationToken cancellationToken);

    /// <summary>Every unread notification for a person, so they can be marked in one go.</summary>
    Task<IReadOnlyList<Notification>> ListUnreadAsync(Guid recipientId, CancellationToken cancellationToken);
}
