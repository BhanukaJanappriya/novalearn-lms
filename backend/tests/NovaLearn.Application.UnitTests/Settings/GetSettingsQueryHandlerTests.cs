using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Settings.Common;
using NovaLearn.Application.Features.Settings.GetSettings;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Settings;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Settings;

public sealed class GetSettingsQueryHandlerTests
{
    private readonly ISettingsRepository _settings = Substitute.For<ISettingsRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly Guid _callerId = Guid.NewGuid();
    private readonly GetSettingsQueryHandler _sut;

    public GetSettingsQueryHandlerTests()
    {
        _sut = new GetSettingsQueryHandler(_settings, _currentUser);
        _settings.GetAsync(Arg.Any<CancellationToken>()).Returns(PlatformSettings.CreateDefault());
    }

    private void SignedInAs(params string[] roles)
    {
        _currentUser.UserId.Returns(_callerId);
        _currentUser.IsInRole(Arg.Any<string>()).Returns(call => roles.Contains(call.Arg<string>()));
    }

    private Task<Result<PlatformSettingsDto>> Act() => _sut.Handle(new GetSettingsQuery(), CancellationToken.None);

    [Fact]
    public async Task An_anonymous_caller_is_refused()
    {
        _currentUser.UserId.Returns((Guid?)null);

        Result<PlatformSettingsDto> result = await Act();

        result.Error.Should().Be(SettingsErrors.Unauthenticated);
    }

    [Fact]
    public async Task A_student_may_not_view_platform_settings()
    {
        SignedInAs(Roles.Student);

        Result<PlatformSettingsDto> result = await Act();

        result.Error.Should().Be(SettingsErrors.ForbiddenToView);
    }

    [Theory]
    [InlineData(Roles.Administrator)]
    [InlineData(Roles.SuperAdministrator)]
    public async Task Either_administrator_role_may_view_settings(string role)
    {
        SignedInAs(role);

        Result<PlatformSettingsDto> result = await Act();

        result.IsSuccess.Should().BeTrue();
    }
}
