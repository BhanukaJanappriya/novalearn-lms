using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Authentication.Common;
using NovaLearn.Application.Features.Authentication.Login;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Authentication;

public sealed class LoginCommandHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IAuthTokenIssuer _tokenIssuer = Substitute.For<IAuthTokenIssuer>();
    private readonly LoginCommandHandler _sut;

    public LoginCommandHandlerTests() => _sut = new LoginCommandHandler(_identityService, _tokenIssuer);

    private static AuthenticatedUser SampleUser() =>
        new(Guid.NewGuid(), "ada@novalearn.local", "Ada", "Lovelace", EmailConfirmed: true, Roles: ["Student"]);

    [Fact]
    public async Task Valid_credentials_issue_tokens_and_record_login()
    {
        AuthenticatedUser user = SampleUser();
        var expected = new AuthenticationResponse(
            "access-token", "refresh-token", DateTimeOffset.UtcNow.AddMinutes(15),
            new UserSummary(user.Id, user.Email, user.FullName, user.Roles));

        _identityService
            .ValidateCredentialsAsync(user.Email, "Str0ng!Pass", Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        _tokenIssuer.IssueAsync(user, Arg.Any<CancellationToken>()).Returns(expected);

        Result<AuthenticationResponse> result =
            await _sut.Handle(new LoginCommand(user.Email, "Str0ng!Pass"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
        await _identityService.Received(1).RecordSuccessfulLoginAsync(user.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Invalid_credentials_fail_without_issuing_tokens()
    {
        _identityService
            .ValidateCredentialsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<AuthenticatedUser>(AuthenticationErrors.InvalidCredentials));

        Result<AuthenticationResponse> result =
            await _sut.Handle(new LoginCommand("nobody@novalearn.local", "wrong"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthenticationErrors.InvalidCredentials);
        await _tokenIssuer.DidNotReceive().IssueAsync(Arg.Any<AuthenticatedUser>(), Arg.Any<CancellationToken>());
    }
}
