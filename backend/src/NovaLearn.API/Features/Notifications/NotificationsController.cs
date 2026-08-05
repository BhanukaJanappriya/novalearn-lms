using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using NovaLearn.API.Common;
using NovaLearn.Application.Features.Notifications.Common;
using NovaLearn.Application.Features.Notifications.GetNotifications;
using NovaLearn.Application.Features.Notifications.GetUnreadCount;
using NovaLearn.Application.Features.Notifications.MarkRead;
using NovaLearn.Shared.Common;

namespace NovaLearn.API.Features.Notifications;

/// <summary>
/// The notification feed. Every action is scoped to the caller's own notifications, so there is
/// no user id in any route: you can only ever read or clear your own.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/notifications")]
[Authorize]
public sealed class NotificationsController(ISender sender) : ApiControllerBase
{
    /// <summary>The caller's feed, newest first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        HandleResult(await sender.Send(new GetNotificationsQuery(unreadOnly, page, pageSize), cancellationToken));

    /// <summary>How many are unread, for the badge.</summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(UnreadCountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UnreadCount(CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetUnreadCountQuery(), cancellationToken));

    /// <summary>Marks one notification read.</summary>
    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new MarkNotificationReadCommand(id), cancellationToken));

    /// <summary>Clears the caller's whole unread queue.</summary>
    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new MarkAllNotificationsReadCommand(), cancellationToken));
}
