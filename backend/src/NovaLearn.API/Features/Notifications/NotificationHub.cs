using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace NovaLearn.API.Features.Notifications;

/// <summary>
/// The live notification channel. Authenticated like every other endpoint, and deliberately
/// one-way: the hub exposes no methods a client can call, so a connected browser can only
/// receive. Anything a client wants to change goes through the REST controller, where the
/// existing authorisation and validation apply.
///
/// Each connection joins a group named after its own user id, so a push is addressed to a
/// person rather than a connection, and someone signed in on two devices sees it on both.
/// </summary>
[Authorize]
public sealed class NotificationHub : Hub
{
    /// <summary>Group name for one person's connections.</summary>
    public static string GroupFor(Guid userId) => $"user:{userId}";

    public override async Task OnConnectedAsync()
    {
        if (TryGetUserId(out Guid userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupFor(userId));
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (TryGetUserId(out Guid userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupFor(userId));
        }

        await base.OnDisconnectedAsync(exception);
    }

    private bool TryGetUserId(out Guid userId) =>
        Guid.TryParse(Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub), out userId);
}
