using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Notifications.GetUnreadCount;

/// <summary>How many unread notifications the caller has, for the badge.</summary>
public sealed record GetUnreadCountQuery : IRequest<Result<UnreadCountDto>>;

public sealed record UnreadCountDto(int UnreadCount);

public sealed class GetUnreadCountQueryHandler(
    INotificationRepository notifications,
    ICurrentUser currentUser)
    : IRequestHandler<GetUnreadCountQuery, Result<UnreadCountDto>>
{
    public async Task<Result<UnreadCountDto>> Handle(
        GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } recipientId)
        {
            return Result.Failure<UnreadCountDto>(NotificationErrors.Unauthenticated);
        }

        return new UnreadCountDto(await notifications.CountUnreadAsync(recipientId, cancellationToken));
    }
}
