using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Admin.Users.Common;
using NovaLearn.Application.Features.Admin.Users.UpdateUserRoles;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Admin;

public sealed class UpdateUserRolesCommandHandlerTests
{
    private readonly IUserDirectory _directory = Substitute.For<IUserDirectory>();
    private readonly IUserAdministration _users = Substitute.For<IUserAdministration>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly Guid _callerId = Guid.NewGuid();
    private readonly Guid _targetId = Guid.NewGuid();
    private readonly UpdateUserRolesCommandHandler _sut;

    public UpdateUserRolesCommandHandlerTests()
    {
        _sut = new UpdateUserRolesCommandHandler(_directory, _users, _currentUser);
        _users.SetRolesAsync(Arg.Any<Guid>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
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
    public async Task Promoting_a_student_to_teaching_assistant_persists_the_new_set()
    {
        SignedInAs(Roles.Administrator);
        TargetHas(Roles.Student);

        Result<AdminUserDto> result = await _sut.Handle(
            new UpdateUserRolesCommand(_targetId, [Roles.Student, Roles.TeachingAssistant]),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _users.Received(1).SetRolesAsync(
            _targetId,
            Arg.Is<IReadOnlyList<string>>(r => r.Contains(Roles.TeachingAssistant) && r.Contains(Roles.Student)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_missing_account_is_reported_as_not_found()
    {
        SignedInAs(Roles.Administrator);
        _directory.GetAsync(_targetId, Arg.Any<CancellationToken>()).Returns((AdminUserRow?)null);

        Result<AdminUserDto> result = await _sut.Handle(
            new UpdateUserRolesCommand(_targetId, [Roles.Student]), CancellationToken.None);

        result.Error.Should().Be(UserAdminErrors.NotFound);
        await _users.DidNotReceive().SetRolesAsync(
            Arg.Any<Guid>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Escalation_is_refused_before_anything_is_written()
    {
        SignedInAs(Roles.Administrator);
        TargetHas(Roles.Student);

        Result<AdminUserDto> result = await _sut.Handle(
            new UpdateUserRolesCommand(_targetId, [Roles.SuperAdministrator]), CancellationToken.None);

        result.Error.Should().Be(UserAdminErrors.CannotGrantSuperAdmin);
        await _users.DidNotReceive().SetRolesAsync(
            Arg.Any<Guid>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Reachable only when another super administrator performs the demotion, which is why it
    /// cannot be exercised through the API against a single-super-admin database.
    /// </summary>
    [Fact]
    public async Task Demoting_the_last_super_administrator_is_refused()
    {
        SignedInAs(Roles.SuperAdministrator);
        TargetHas(Roles.SuperAdministrator);
        _directory.CountInRoleAsync(Roles.SuperAdministrator, Arg.Any<CancellationToken>()).Returns(1);

        Result<AdminUserDto> result = await _sut.Handle(
            new UpdateUserRolesCommand(_targetId, [Roles.Administrator]), CancellationToken.None);

        result.Error.Should().Be(UserAdminErrors.LastSuperAdmin);
        await _users.DidNotReceive().SetRolesAsync(
            Arg.Any<Guid>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Demoting_a_super_administrator_is_allowed_while_another_remains()
    {
        SignedInAs(Roles.SuperAdministrator);
        TargetHas(Roles.SuperAdministrator);
        _directory.CountInRoleAsync(Roles.SuperAdministrator, Arg.Any<CancellationToken>()).Returns(2);

        Result<AdminUserDto> result = await _sut.Handle(
            new UpdateUserRolesCommand(_targetId, [Roles.Administrator]), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task An_identity_failure_is_surfaced_rather_than_swallowed()
    {
        SignedInAs(Roles.Administrator);
        TargetHas(Roles.Student);
        _users.SetRolesAsync(Arg.Any<Guid>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(UserAdminErrors.Identity("Role does not exist.")));

        Result<AdminUserDto> result = await _sut.Handle(
            new UpdateUserRolesCommand(_targetId, [Roles.Lecturer]), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("user_admin.identity_error");
    }
}
