using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Notifications;
using NovaLearn.Shared.Common;

namespace NovaLearn.Persistence.Repositories;

public sealed class NotificationRepository(ApplicationDbContext dbContext) : INotificationRepository
{
    public async Task AddRangeAsync(
        IEnumerable<Notification> notifications, CancellationToken cancellationToken) =>
        await dbContext.Notifications.AddRangeAsync(notifications, cancellationToken);

    public async Task<PagedResult<Notification>> ListAsync(
        Guid recipientId, bool unreadOnly, int page, int pageSize, CancellationToken cancellationToken)
    {
        int safePage = page < 1 ? 1 : page;
        int safePageSize = pageSize < 1 ? 20 : pageSize;

        IQueryable<Notification> query = dbContext.Notifications
            .Where(n => n.RecipientId == recipientId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<Notification> items = await query
            .OrderByDescending(n => n.CreatedAtUtc)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Notification>(items, safePage, safePageSize, totalCount);
    }

    public Task<Notification?> GetAsync(Guid notificationId, CancellationToken cancellationToken) =>
        dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId, cancellationToken);

    public Task<int> CountUnreadAsync(Guid recipientId, CancellationToken cancellationToken) =>
        dbContext.Notifications.CountAsync(n => n.RecipientId == recipientId && !n.IsRead, cancellationToken);

    public async Task<IReadOnlyList<Notification>> ListUnreadAsync(
        Guid recipientId, CancellationToken cancellationToken) =>
        await dbContext.Notifications
            .Where(n => n.RecipientId == recipientId && !n.IsRead)
            .ToListAsync(cancellationToken);
}
