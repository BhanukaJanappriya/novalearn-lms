using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Security.RevokeSession;
using NovaLearn.Domain.Audit;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Security;

public sealed class RevokeSessionCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IAuditLogger _auditLogger = Substitute.For<IAuditLogger>();
    private readonly RevokeSessionCommandHandler _sut;
    private readonly Guid _callerId = Guid.NewGuid();

    public RevokeSessionCommandHandlerTests()
    {
        _sut = new RevokeSessionCommandHandler(_refreshTokens, _currentUser, _clock, _unitOfWork, _auditLogger);
        _clock.UtcNow.Returns(Now);
        _currentUser.UserId.Returns(_callerId);
        _currentUser.IsInRole(Roles.Administrator).Returns(true);
    }

    private static RefreshToken ActiveSession(Guid userId) =>
        RefreshToken.Issue(userId, "hash", "jti", Now.AddDays(-1), Now.AddDays(6), "203.0.113.5");

    [Fact]
    public async Task A_non_staff_caller_is_refused()
    {
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);

        Result result = await _sut.Handle(new RevokeSessionCommand(Guid.NewGuid()), CancellationToken.None);

        result.Error.Should().Be(SecurityErrors.StaffOnly);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_missing_session_is_reported()
    {
        Guid id = Guid.NewGuid();
        _refreshTokens.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);

        Result result = await _sut.Handle(new RevokeSessionCommand(id), CancellationToken.None);

        result.Error.Should().Be(SecurityErrors.SessionNotFound);
    }

    [Fact]
    public async Task An_already_revoked_session_is_refused()
    {
        RefreshToken session = ActiveSession(Guid.NewGuid());
        session.Revoke(Now.AddMinutes(-1), null);
        _refreshTokens.GetByIdAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);

        Result result = await _sut.Handle(new RevokeSessionCommand(session.Id), CancellationToken.None);

        result.Error.Should().Be(SecurityErrors.SessionNotActive);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_successful_revoke_saves_and_logs_itself()
    {
        Guid userId = Guid.NewGuid();
        RefreshToken session = ActiveSession(userId);
        _refreshTokens.GetByIdAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);

        Result result = await _sut.Handle(new RevokeSessionCommand(session.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        session.IsRevoked.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _auditLogger.Received(1).RecordAsync(
            _callerId, AuditCategory.Security, "Revoked session", Arg.Any<string>(), "User", userId,
            Arg.Any<CancellationToken>());
    }
}
