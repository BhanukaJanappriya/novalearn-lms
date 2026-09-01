using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Security.RevokeAllSessionsForUser;
using NovaLearn.Domain.Audit;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Security;

public sealed class RevokeAllSessionsForUserCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly IUserDirectory _directory = Substitute.For<IUserDirectory>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IAuditLogger _auditLogger = Substitute.For<IAuditLogger>();
    private readonly RevokeAllSessionsForUserCommandHandler _sut;
    private readonly Guid _callerId = Guid.NewGuid();
    private readonly Guid _targetId = Guid.NewGuid();

    public RevokeAllSessionsForUserCommandHandlerTests()
    {
        _sut = new RevokeAllSessionsForUserCommandHandler(
            _refreshTokens, _directory, _currentUser, _clock, _unitOfWork, _auditLogger);
        _clock.UtcNow.Returns(Now);
        _currentUser.UserId.Returns(_callerId);
        _currentUser.IsInRole(Roles.Administrator).Returns(true);
    }

    private static AdminUserRow TargetUser(Guid id) => new(
        id, "Jane", "Learner", "jane@example.com", null, true, true, false,
        Now.AddDays(-30), Now.AddDays(-1), ["Student"], 0, 0);

    [Fact]
    public async Task A_non_staff_caller_is_refused()
    {
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);

        Result result = await _sut.Handle(new RevokeAllSessionsForUserCommand(_targetId), CancellationToken.None);

        result.Error.Should().Be(SecurityErrors.StaffOnly);
        await _refreshTokens.DidNotReceive().RevokeAllActiveForUserAsync(
            Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_missing_account_is_reported()
    {
        _directory.GetAsync(_targetId, Arg.Any<CancellationToken>()).Returns((AdminUserRow?)null);

        Result result = await _sut.Handle(new RevokeAllSessionsForUserCommand(_targetId), CancellationToken.None);

        result.Error.Should().Be(UserAdminErrors.NotFound);
    }

    [Fact]
    public async Task A_successful_revoke_saves_and_logs_itself()
    {
        _directory.GetAsync(_targetId, Arg.Any<CancellationToken>()).Returns(TargetUser(_targetId));

        Result result = await _sut.Handle(new RevokeAllSessionsForUserCommand(_targetId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _refreshTokens.Received(1).RevokeAllActiveForUserAsync(
            _targetId, Now, Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _auditLogger.Received(1).RecordAsync(
            _callerId, AuditCategory.Security, "Revoked all sessions", "Jane Learner", "User", _targetId,
            Arg.Any<CancellationToken>());
    }
}
