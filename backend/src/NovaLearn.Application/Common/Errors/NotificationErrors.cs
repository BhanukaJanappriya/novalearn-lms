using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Common.Errors;

/// <summary>Central catalogue of notification failures.</summary>
public static class NotificationErrors
{
    public static readonly Error NotFound =
        Error.NotFound("notification.not_found", "The requested notification was not found.");

    public static readonly Error Unauthenticated =
        Error.Unauthorized("notification.unauthenticated", "You must be signed in to read notifications.");

    public static readonly Error NotRecipient =
        Error.Forbidden("notification.not_recipient", "You can only manage your own notifications.");
}
