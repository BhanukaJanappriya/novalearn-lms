using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Profile.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Profile.GetMyProfile;

/// <summary>
/// The caller's own profile, read from storage rather than token claims, so it reflects an
/// avatar changed since the token was issued.
/// </summary>
public sealed record GetMyProfileQuery : IRequest<Result<MyProfileDto>>;

public sealed class GetMyProfileQueryHandler(IProfileService profiles, ICurrentUser currentUser)
    : IRequestHandler<GetMyProfileQuery, Result<MyProfileDto>>
{
    public async Task<Result<MyProfileDto>> Handle(
        GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure<MyProfileDto>(ProfileErrors.Unauthenticated);
        }

        MyProfileDto? profile = await profiles.GetAsync(userId, cancellationToken);

        return profile is null
            ? Result.Failure<MyProfileDto>(ProfileErrors.NotFound)
            : profile;
    }
}
