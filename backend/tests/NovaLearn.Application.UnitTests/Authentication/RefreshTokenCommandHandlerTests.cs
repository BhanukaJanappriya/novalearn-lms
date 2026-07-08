using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Authentication.Common;
using NovaLearn.Application.Features.Authentication.RefreshToken;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;
using NovaLearn.Shared.Security;
using Xunit;

namespace NovaLearn.Application.UnitTests.Authentication;

public sealed class RefreshTokenCommandHandlerTests
{
    private readonly IRefreshTokenRepository _repository = Substitute.For<IRefreshTokenRepository>();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IAuthTokenIssuer _tokenIssuer = Substitute.For<IAuthTokenIssuer>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly RefreshTokenCommandHandler _sut;

    public RefreshTokenCommandHandlerTests()
    {
        _clock.UtcNow.Returns(DateTimeOffset.UtcNow);
        _sut = new RefreshTokenCommandHandler(
            _repository, _identityService, _tokenIssuer, _unitOfWork, _clock, _currentUser,
            Substitute.For<ILogger<RefreshTokenCommandHandler>>());
    }

    [Fact]
    public async Task Unknown_token_is_rejected()
    {
        _repository.GetByHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        Result<AuthenticationResponse> result =
            await _sut.Handle(new RefreshTokenCommand("does-not-exist"), CancellationToken.None);

        result.Error.Should().Be(AuthenticationErrors.InvalidRefreshToken);
    }

    [Fact]
    public async Task Replayed_revoked_token_revokes_entire_chain()
    {
        const string raw = "compromised-token";
        Guid userId = Guid.NewGuid();

        RefreshToken revoked = RefreshToken.Issue(
            userId, TokenHasher.Hash(raw), "jwt-id",
            _clock.UtcNow.AddMinutes(-10), _clock.UtcNow.AddDays(7), "127.0.0.1");
        revoked.Revoke(_clock.UtcNow.AddMinutes(-5), "127.0.0.1");

        _repository.GetByHashAsync(TokenHasher.Hash(raw), Arg.Any<CancellationToken>()).Returns(revoked);

        Result<AuthenticationResponse> result =
            await _sut.Handle(new RefreshTokenCommand(raw), CancellationToken.None);

        result.Error.Should().Be(AuthenticationErrors.InvalidRefreshToken);
        await _repository.Received(1).RevokeAllActiveForUserAsync(
            userId, Arg.Any<DateTimeOffset>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _tokenIssuer.DidNotReceive().IssueAsync(Arg.Any<AuthenticatedUser>(), Arg.Any<CancellationToken>());
    }
}
