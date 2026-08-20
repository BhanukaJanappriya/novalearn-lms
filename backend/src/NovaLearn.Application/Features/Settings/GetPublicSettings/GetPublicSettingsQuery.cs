using MediatR;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Settings.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Settings.GetPublicSettings;

/// <summary>
/// The handful of settings an anonymous visitor may see: branding, and whether the platform is
/// presently in maintenance. Open to everyone on purpose — the maintenance banner has to render
/// for the very visitors maintenance mode is blocking, and the sign-in page needs the site name
/// before anyone has signed in.
/// </summary>
public sealed record GetPublicSettingsQuery : IRequest<Result<PublicSettingsDto>>;

public sealed class GetPublicSettingsQueryHandler(ISettingsProvider settings)
    : IRequestHandler<GetPublicSettingsQuery, Result<PublicSettingsDto>>
{
    public async Task<Result<PublicSettingsDto>> Handle(
        GetPublicSettingsQuery request, CancellationToken cancellationToken)
    {
        PlatformSettingsSnapshot snapshot = await settings.GetAsync(cancellationToken);

        return Result.Success(new PublicSettingsDto(
            snapshot.SiteName,
            snapshot.SupportEmail,
            snapshot.AllowNewRegistrations,
            snapshot.MaintenanceModeEnabled,
            snapshot.MaintenanceMessage));
    }
}
