using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Notifications.Common;
using NovaLearn.Domain.Notifications;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Notifications.GetNotifications;

public sealed class GetNotificationsQueryHandler(
    INotificationRepository notifications,
    ICurrentUser currentUser)
    : IRequestHandler<GetNotificationsQuery, Result<PagedResult<NotificationDto>>>
{
    public async Task<Result<PagedResult<NotificationDto>>> Handle(
        GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } recipientId)
        {
            return Result.Failure<PagedResult<NotificationDto>>(NotificationErrors.Unauthenticated);
        }

        PagedResult<Notification> page = await notifications.ListAsync(
            recipientId, request.UnreadOnly, request.Page, request.PageSize, cancellationToken);

        return new PagedResult<NotificationDto>(
            page.Items.Select(NotificationDto.FromEntity).ToList(),
            page.Page,
            page.PageSize,
            page.TotalCount);
    }
}
