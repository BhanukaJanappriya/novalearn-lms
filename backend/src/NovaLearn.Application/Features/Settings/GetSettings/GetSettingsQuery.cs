using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Settings.Common;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Settings;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Settings.GetSettings;

/// <summary>The full settings row, for the admin screen. Viewable by any administrator.</summary>
public sealed record GetSettingsQuery : IRequest<Result<PlatformSettingsDto>>;

public sealed class GetSettingsQueryHandler(ISettingsRepository settings, ICurrentUser currentUser)
    : IRequestHandler<GetSettingsQuery, Result<PlatformSettingsDto>>
{
    public async Task<Result<PlatformSettingsDto>> Handle(
        GetSettingsQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure<PlatformSettingsDto>(SettingsErrors.Unauthenticated);
        }

        if (!currentUser.IsInRole(Roles.Administrator) && !currentUser.IsInRole(Roles.SuperAdministrator))
        {
            return Result.Failure<PlatformSettingsDto>(SettingsErrors.ForbiddenToView);
        }

        PlatformSettings current = await settings.GetAsync(cancellationToken);

        return Result.Success(PlatformSettingsMapper.ToDto(current));
    }
}
