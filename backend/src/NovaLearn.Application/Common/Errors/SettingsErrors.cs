using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Common.Errors;

/// <summary>Central catalogue of platform-settings failures.</summary>
public static class SettingsErrors
{
    public static readonly Error Unauthenticated =
        Error.Unauthorized("settings.unauthenticated", "You must be signed in to view platform settings.");

    public static readonly Error ForbiddenToView =
        Error.Forbidden("settings.forbidden_view", "Only administrators can view platform settings.");

    public static readonly Error ForbiddenToEdit =
        Error.Forbidden(
            "settings.forbidden_edit",
            "Only a super administrator can change platform settings — this affects everyone on the platform.");
}
