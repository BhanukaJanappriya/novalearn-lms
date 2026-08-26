using FluentValidation;
using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Settings.Common;
using NovaLearn.Domain.Audit;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Settings;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Settings.UpdateSettings;

/// <summary>Edits platform settings. Restricted to super administrators — see the handler for why.</summary>
public sealed record UpdateSettingsCommand(
    string SiteName,
    string SupportEmail,
    bool AllowNewRegistrations,
    bool MaintenanceModeEnabled,
    string? MaintenanceMessage,
    string DefaultCurrency,
    int MaxUploadSizeMb) : IRequest<Result<PlatformSettingsDto>>;

public sealed class UpdateSettingsCommandValidator : AbstractValidator<UpdateSettingsCommand>
{
    public UpdateSettingsCommandValidator()
    {
        RuleFor(command => command.SiteName).NotEmpty().MaximumLength(100);

        RuleFor(command => command.SupportEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(command => command.MaintenanceMessage).MaximumLength(500);

        RuleFor(command => command.DefaultCurrency)
            .NotEmpty()
            .Matches("^[A-Za-z]{3}$")
            .WithMessage("Currency must be a three letter ISO code, such as usd or eur.");

        RuleFor(command => command.MaxUploadSizeMb).InclusiveBetween(1, 500);
    }
}

/// <summary>
/// Restricted to super administrators rather than the broader administrator role every other
/// admin screen accepts. Registration and maintenance mode are platform-wide switches: flipping
/// either affects every learner and every other administrator, not one course or one account, so
/// this sits with the same authority that can act on another administrator's own account.
/// </summary>
public sealed class UpdateSettingsCommandHandler(
    ISettingsRepository settings,
    ISettingsProvider provider,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IAuditLogger auditLogger)
    : IRequestHandler<UpdateSettingsCommand, Result<PlatformSettingsDto>>
{
    public async Task<Result<PlatformSettingsDto>> Handle(
        UpdateSettingsCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure<PlatformSettingsDto>(SettingsErrors.Unauthenticated);
        }

        if (!currentUser.IsInRole(Roles.SuperAdministrator))
        {
            return Result.Failure<PlatformSettingsDto>(SettingsErrors.ForbiddenToEdit);
        }

        PlatformSettings current = await settings.GetAsync(cancellationToken);

        current.Update(
            request.SiteName,
            request.SupportEmail,
            request.AllowNewRegistrations,
            request.MaintenanceModeEnabled,
            request.MaintenanceMessage,
            request.DefaultCurrency,
            request.MaxUploadSizeMb);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Every other reader of settings goes through the cached provider, which must not keep
        // answering with what was true a moment ago — maintenance mode in particular is meant to
        // take effect the instant it is switched on.
        provider.Invalidate();

        await auditLogger.RecordAsync(
            currentUser.UserId!.Value,
            AuditCategory.Settings,
            "Updated platform settings",
            $"Maintenance mode: {(request.MaintenanceModeEnabled ? "on" : "off")}, "
                + $"registrations: {(request.AllowNewRegistrations ? "open" : "closed")}",
            "PlatformSettings",
            current.Id,
            cancellationToken);

        return Result.Success(PlatformSettingsMapper.ToDto(current));
    }
}
