using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Authentication.Register;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Authentication;

public sealed class RegisterCommandHandlerTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly ISettingsProvider _settings = Substitute.For<ISettingsProvider>();
    private readonly RegisterCommandHandler _sut;

    public RegisterCommandHandlerTests()
    {
        _sut = new RegisterCommandHandler(
            _identity, _emailSender, _settings, Substitute.For<ILogger<RegisterCommandHandler>>());

        _settings.GetAsync(Arg.Any<CancellationToken>()).Returns(
            new PlatformSettingsSnapshot("NovaLearn", "support@novalearn.local", true, false, null, "usd", 200));

        _identity.CreateUserAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new AuthenticatedUser(
                Guid.NewGuid(), "ada@novalearn.local", "Ada", "Lovelace", false, ["Student"])));

        _identity.GenerateEmailConfirmationTokenAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns("token");
    }

    private static RegisterCommand Command() =>
        new("Ada", "Lovelace", "ada@novalearn.local", "Str0ng!Pass");

    [Fact]
    public async Task Registration_succeeds_while_the_platform_accepts_new_accounts()
    {
        Result<RegisterResponse> result = await _sut.Handle(Command(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _identity.Received(1).CreateUserAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Registration_is_refused_while_the_platform_has_closed_new_accounts()
    {
        _settings.GetAsync(Arg.Any<CancellationToken>()).Returns(
            new PlatformSettingsSnapshot("NovaLearn", "support@novalearn.local", false, false, null, "usd", 200));

        Result<RegisterResponse> result = await _sut.Handle(Command(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthenticationErrors.RegistrationClosed);

        // Checked before anything is created, not after — a closed platform never gets as far as
        // touching the identity store or sending a verification email for a rejected signup.
        await _identity.DidNotReceive().CreateUserAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _emailSender.DidNotReceive().SendEmailVerificationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
