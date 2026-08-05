using MediatR;
using NovaLearn.Application.Features.Notifications.Common;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Notifications.GetNotifications;

/// <summary>The caller's own feed. There is no id parameter, so nobody can read another feed.</summary>
public sealed record GetNotificationsQuery(bool UnreadOnly, int Page, int PageSize)
    : IRequest<Result<PagedResult<NotificationDto>>>;
