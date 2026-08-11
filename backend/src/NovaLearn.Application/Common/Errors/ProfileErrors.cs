using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Common.Errors;

/// <summary>Central catalogue of self-service profile failures.</summary>
public static class ProfileErrors
{
    public static readonly Error NotFound =
        Error.NotFound("profile.not_found", "Your profile could not be found.");

    public static readonly Error Unauthenticated =
        Error.Unauthorized("profile.unauthenticated", "You must be signed in to edit your profile.");

    public static readonly Error InvalidAvatarUrl =
        Error.Validation(
            "profile.invalid_avatar_url",
            "A picture link must be a full http or https web address.");
}
