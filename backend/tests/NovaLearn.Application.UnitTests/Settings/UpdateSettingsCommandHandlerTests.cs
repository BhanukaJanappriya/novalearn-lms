using FluentAssertions;
using FluentValidation.TestHelper;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Settings.Common;
using NovaLearn.Application.Features.Settings.UpdateSettings;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Settings;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Settings;

public sealed class UpdateSettingsCommandHandlerTests
{
    private readonly ISettingsRepository _settings = Substitute.For<ISettingsRepository>();
    private readonly ISettingsProvider _provider = Substitute.For<ISettingsProvider>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly Guid _callerId = Guid.NewGuid();
    private readonly UpdateSettingsCommandHandler _sut;

    public UpdateSettingsCommandHandlerTests()
    {
        _sut = new UpdateSettingsCommandHandler(_settings, _provider, _currentUser, _unitOfWork);
        _settings.GetAsync(Arg.Any<CancellationToken>()).Returns(PlatformSettings.CreateDefault());
    }

    private void SignedInAs(params string[] roles)
    {
        _currentUser.UserId.Returns(_callerId);
        _currentUser.IsInRole(Arg.Any<string>()).Returns(call => roles.Contains(call.Arg<string>()));
    }

    private static UpdateSettingsCommand ValidCommand(
        bool allowNewRegistrations = true,
        bool maintenanceModeEnabled = false,
        string currency = "usd",
        int maxUploadSizeMb = 200) =>
        new("NovaLearn", "support@novalearn.local", allowNewRegistrations, maintenanceModeEnabled,
            null, currency, maxUploadSizeMb);

    private Task<Result<PlatformSettingsDto>> Act(UpdateSettingsCommand command) =>
        _sut.Handle(command, CancellationToken.None);

    [Fact]
    public async Task An_administrator_who_is_not_a_super_administrator_cannot_edit_settings()
    {
        SignedInAs(Roles.Administrator);

        Result<PlatformSettingsDto> result = await Act(ValidCommand());

        result.Error.Should().Be(SettingsErrors.ForbiddenToEdit);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _provider.DidNotReceive().Invalidate();
    }

    [Fact]
    public async Task A_super_administrator_can_edit_settings()
    {
        SignedInAs(Roles.SuperAdministrator);

        Result<PlatformSettingsDto> result = await Act(ValidCommand(maintenanceModeEnabled: true));

        result.IsSuccess.Should().BeTrue();
        result.Value.MaintenanceModeEnabled.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Saving_invalidates_the_cached_settings_so_the_change_takes_effect_at_once()
    {
        SignedInAs(Roles.SuperAdministrator);

        await Act(ValidCommand());

        // Maintenance mode in particular is meant to take effect the instant it is switched, not
        // for however long a cache would otherwise have gone on answering with the old value.
        _provider.Received(1).Invalidate();
    }

    [Fact]
    public async Task An_anonymous_caller_cannot_edit_settings()
    {
        _currentUser.UserId.Returns((Guid?)null);

        Result<PlatformSettingsDto> result = await Act(ValidCommand());

        result.Error.Should().Be(SettingsErrors.Unauthenticated);
    }
}

public sealed class UpdateSettingsCommandValidatorTests
{
    private readonly UpdateSettingsCommandValidator _validator = new();

    private static UpdateSettingsCommand Command(
        string siteName = "NovaLearn",
        string supportEmail = "support@novalearn.local",
        string currency = "usd",
        int maxUploadSizeMb = 200,
        string? maintenanceMessage = null) =>
        new(siteName, supportEmail, true, false, maintenanceMessage, currency, maxUploadSizeMb);

    [Fact]
    public void A_well_formed_command_passes()
    {
        TestValidationResult<UpdateSettingsCommand> result = _validator.TestValidate(Command());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void An_invalid_support_email_fails(string email)
    {
        TestValidationResult<UpdateSettingsCommand> result =
            _validator.TestValidate(Command(supportEmail: email));
        result.ShouldHaveValidationErrorFor(c => c.SupportEmail);
    }

    [Theory]
    [InlineData("us")]
    [InlineData("usdollars")]
    [InlineData("123")]
    public void A_currency_that_is_not_a_three_letter_code_fails(string currency)
    {
        TestValidationResult<UpdateSettingsCommand> result =
            _validator.TestValidate(Command(currency: currency));
        result.ShouldHaveValidationErrorFor(c => c.DefaultCurrency);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(501)]
    public void An_upload_size_outside_the_allowed_range_fails(int megabytes)
    {
        TestValidationResult<UpdateSettingsCommand> result =
            _validator.TestValidate(Command(maxUploadSizeMb: megabytes));
        result.ShouldHaveValidationErrorFor(c => c.MaxUploadSizeMb);
    }

    [Fact]
    public void An_empty_site_name_fails()
    {
        TestValidationResult<UpdateSettingsCommand> result = _validator.TestValidate(Command(siteName: ""));
        result.ShouldHaveValidationErrorFor(c => c.SiteName);
    }
}
