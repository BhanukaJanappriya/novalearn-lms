using NovaLearn.Domain.Notifications;

namespace NovaLearn.Application.Features.Notifications.Common;

/// <summary>One item in the feed, as both the REST endpoint and the live push send it.</summary>
public sealed record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Message,
    string? Link,
    bool IsRead,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ReadAtUtc)
{
    public static NotificationDto FromEntity(Notification notification) => new(
        notification.Id,
        notification.Type.ToString(),
        notification.Title,
        notification.Message,
        notification.Link,
        notification.IsRead,
        notification.CreatedAtUtc,
        notification.ReadAtUtc);
}
