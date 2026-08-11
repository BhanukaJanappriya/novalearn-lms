using FluentValidation;
using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Profile.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Profile.UpdateAvatar;

/// <summary>
/// Sets the caller's own profile picture.
///
/// There is deliberately no user id here. The subject comes from the token, so this command
/// cannot be pointed at anyone else's profile however it is called.
/// </summary>
public sealed record UpdateMyAvatarCommand(string? AvatarUrl) : IRequest<Result<MyProfileDto>>;

public sealed class UpdateMyAvatarCommandValidator : AbstractValidator<UpdateMyAvatarCommand>
{
    public UpdateMyAvatarCommandValidator()
    {
        RuleFor(x => x.AvatarUrl)
            .MaximumLength(2048)
            .Must(BeASafeImageUrl)
            .WithMessage("A picture link must be a full http or https web address.")
            .When(x => !string.IsNullOrWhiteSpace(x.AvatarUrl));
    }

    /// <summary>
    /// Only absolute http and https links are accepted. The value ends up in an img src, so
    /// letting through javascript: or other schemes would hand every viewer of a profile a
    /// script the profile owner chose.
    /// </summary>
    private static bool BeASafeImageUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return Uri.TryCreate(value.Trim(), UriKind.Absolute, out Uri? uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}

public sealed class UpdateMyAvatarCommandHandler(
    IProfileService profiles,
    ICurrentUser currentUser)
    : IRequestHandler<UpdateMyAvatarCommand, Result<MyProfileDto>>
{
    public async Task<Result<MyProfileDto>> Handle(
        UpdateMyAvatarCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure<MyProfileDto>(ProfileErrors.Unauthenticated);
        }

        Result result = await profiles.SetAvatarAsync(userId, request.AvatarUrl, cancellationToken);
        if (result.IsFailure)
        {
            return Result.Failure<MyProfileDto>(result.Error);
        }

        MyProfileDto? profile = await profiles.GetAsync(userId, cancellationToken);

        return profile is null
            ? Result.Failure<MyProfileDto>(ProfileErrors.NotFound)
            : profile;
    }
}
