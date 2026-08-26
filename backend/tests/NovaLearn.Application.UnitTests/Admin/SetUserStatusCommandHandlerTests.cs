using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Admin.Users.Common;
using NovaLearn.Application.Features.Admin.Users.SetUserStatus;
using NovaLearn.Domain.Audit;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Admin;

public sealed class SetUserStatusCommandHandlerTests
{
    private readonly IUserDirectory _directory = Substitute.For<IUserDirectory>();
    private readonly IUserAdministration _users = Substitute.For<IUserAdministration>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IAuditLogger _auditLogger = Substitute.For<IAuditLogger>();
    private readonly Guid _callerId = Guid.NewGuid();
    private readonly Guid _targetId = Guid.NewGuid();
    private readonly SetUserStatusCommandHandler _sut;

    public SetUserStatusCommandHandlerTests()
    {
        _sut = new SetUserStatusCommandHandler(_directory, _users, _currentUser, _auditLogger);
        _users.SetActiveAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
    }

    private void SignedInAs(params string[] roles)
    {
        _currentUser.UserId.Returns(_callerId);
        _currentUser.IsInRole(Arg.Any<string>()).Returns(call => roles.Contains(call.Arg<string>()));
    }

    private void TargetHas(params string[] roles) =>
        _directory.GetAsync(_targetId, Arg.Any<CancellationToken>())
            .Returns(UserAdminTestData.User(_targetId, roles));

    [Fact]
    public async Task An_administrator_can_deactivate_an_ordinary_account()
    {
        SignedInAs(Roles.Administrator);
        TargetHas(Roles.Student);

        Result<AdminUserDto> result = await _sut.Handle(
            new SetUserStatusCommand(_targetId, false), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _users.Received(1).SetActiveAsync(_targetId, false, Arg.Any<CancellationToken>());
        await _auditLogger.Received(1).RecordAsync(
            _callerId, AuditCategory.UserManagement, "Deactivated account", Arg.Any<string>(), "User", _targetId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deactivating_yourself_is_refused()
    {
        SignedInAs(Roles.SuperAdministrator);
        _directory.GetAsync(_callerId, Arg.Any<CancellationToken>())
            .Returns(UserAdminTestData.User(_callerId, Roles.SuperAdministrator));

        Result<AdminUserDto> result = await _sut.Handle(
            new SetUserStatusCommand(_callerId, false), CancellationToken.None);

        result.Error.Should().Be(UserAdminErrors.CannotModifySelf);
        await _users.DidNotReceive().SetActiveAsync(
            Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deactivating_the_last_super_administrator_is_refused()
    {
        SignedInAs(Roles.SuperAdministrator);
        TargetHas(Roles.SuperAdministrator);
        _directory.CountInRoleAsync(Roles.SuperAdministrator, Arg.Any<CancellationToken>()).Returns(1);

        Result<AdminUserDto> result = await _sut.Handle(
            new SetUserStatusCommand(_targetId, false), CancellationToken.None);

        result.Error.Should().Be(UserAdminErrors.LastSuperAdmin);
        await _users.DidNotReceive().SetActiveAsync(
            Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reactivating_a_super_administrator_skips_the_stranding_check()
    {
        SignedInAs(Roles.SuperAdministrator);
        TargetHas(Roles.SuperAdministrator);
        _directory.CountInRoleAsync(Roles.SuperAdministrator, Arg.Any<CancellationToken>()).Returns(1);

        Result<AdminUserDto> result = await _sut.Handle(
            new SetUserStatusCommand(_targetId, true), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _directory.DidNotReceive()
            .CountInRoleAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_ordinary_administrator_cannot_deactivate_a_super_administrator()
    {
        SignedInAs(Roles.Administrator);
        TargetHas(Roles.SuperAdministrator);

        Result<AdminUserDto> result = await _sut.Handle(
            new SetUserStatusCommand(_targetId, false), CancellationToken.None);

        result.Error.Should().Be(UserAdminErrors.SuperAdminOnly);
    }
}
