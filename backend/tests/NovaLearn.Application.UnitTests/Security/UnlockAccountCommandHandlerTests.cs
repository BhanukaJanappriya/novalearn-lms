using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Security.UnlockAccount;
using NovaLearn.Domain.Audit;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Security;

public sealed class UnlockAccountCommandHandlerTests
{
    private readonly IUserAdministration _users = Substitute.For<IUserAdministration>();
    private readonly IUserDirectory _directory = Substitute.For<IUserDirectory>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IAuditLogger _auditLogger = Substitute.For<IAuditLogger>();
    private readonly UnlockAccountCommandHandler _sut;
    private readonly Guid _callerId = Guid.NewGuid();
    private readonly Guid _targetId = Guid.NewGuid();

    public UnlockAccountCommandHandlerTests()
    {
        _sut = new UnlockAccountCommandHandler(_users, _directory, _currentUser, _auditLogger);
        _currentUser.UserId.Returns(_callerId);
        _currentUser.IsInRole(Roles.Administrator).Returns(true);
        _users.ClearLockoutAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
    }

    private static AdminUserRow TargetUser(Guid id) => new(
        id, "Jane", "Learner", "jane@example.com", null, true, true, true,
        DateTimeOffset.UtcNow.AddDays(-30), null, ["Student"], 0, 0);

    [Fact]
    public async Task A_non_staff_caller_is_refused()
    {
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);

        Result result = await _sut.Handle(new UnlockAccountCommand(_targetId), CancellationToken.None);

        result.Error.Should().Be(SecurityErrors.StaffOnly);
        await _users.DidNotReceive().ClearLockoutAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_missing_account_is_reported()
    {
        _directory.GetAsync(_targetId, Arg.Any<CancellationToken>()).Returns((AdminUserRow?)null);

        Result result = await _sut.Handle(new UnlockAccountCommand(_targetId), CancellationToken.None);

        result.Error.Should().Be(UserAdminErrors.NotFound);
    }

    [Fact]
    public async Task An_identity_failure_is_surfaced_without_logging()
    {
        _directory.GetAsync(_targetId, Arg.Any<CancellationToken>()).Returns(TargetUser(_targetId));
        _users.ClearLockoutAsync(_targetId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(UserAdminErrors.Identity("boom")));

        Result result = await _sut.Handle(new UnlockAccountCommand(_targetId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        await _auditLogger.DidNotReceive().RecordAsync(
            Arg.Any<Guid>(), Arg.Any<AuditCategory>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_successful_unlock_logs_itself()
    {
        _directory.GetAsync(_targetId, Arg.Any<CancellationToken>()).Returns(TargetUser(_targetId));

        Result result = await _sut.Handle(new UnlockAccountCommand(_targetId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _auditLogger.Received(1).RecordAsync(
            _callerId, AuditCategory.Security, "Unlocked account", "Jane Learner", "User", _targetId,
            Arg.Any<CancellationToken>());
    }
}
