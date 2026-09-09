using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Admin.Users.Common;
using NovaLearn.Application.Features.Admin.Users.CreateUser;
using NovaLearn.Domain.Audit;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Admin;

public sealed class CreateUserCommandHandlerTests
{
    private readonly IUserAdministration _users = Substitute.For<IUserAdministration>();
    private readonly IUserDirectory _directory = Substitute.For<IUserDirectory>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IAuditLogger _auditLogger = Substitute.For<IAuditLogger>();
    private readonly Guid _callerId = Guid.NewGuid();
    private readonly Guid _newUserId = Guid.NewGuid();
    private readonly CreateUserCommandHandler _sut;

    public CreateUserCommandHandlerTests()
    {
        _sut = new CreateUserCommandHandler(_users, _directory, _currentUser, _auditLogger);

        _users.CreateAccountAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(_newUserId));

        _directory.GetAsync(_newUserId, Arg.Any<CancellationToken>())
            .Returns(UserAdminTestData.User(_newUserId, Roles.Lecturer));
    }

    private void SignedInAs(params string[] roles)
    {
        _currentUser.UserId.Returns(_callerId);
        _currentUser.IsInRole(Arg.Any<string>()).Returns(call => roles.Contains(call.Arg<string>()));
    }

    private static CreateUserCommand Command(string role = Roles.Lecturer) =>
        new("Grace", "Hopper", "grace.hopper@novalearn.local", role, "Str0ng-Pass12");

    [Fact]
    public async Task An_administrator_can_add_a_lecturer()
    {
        SignedInAs(Roles.Administrator);

        Result<AdminUserDto> result = await _sut.Handle(Command(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _users.Received(1).CreateAccountAsync(
            "grace.hopper@novalearn.local", "Grace", "Hopper", Roles.Lecturer, "Str0ng-Pass12",
            Arg.Any<CancellationToken>());
        await _auditLogger.Received(1).RecordAsync(
            _callerId, AuditCategory.UserManagement, "Created account", Arg.Any<string>(), "User",
            _newUserId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_unauthenticated_caller_is_refused_before_anything_is_created()
    {
        _currentUser.UserId.Returns((Guid?)null);

        Result<AdminUserDto> result = await _sut.Handle(Command(), CancellationToken.None);

        result.Error.Should().Be(UserAdminErrors.Unauthenticated);
        await _users.DidNotReceive().CreateAccountAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_ordinary_administrator_cannot_mint_a_super_administrator()
    {
        SignedInAs(Roles.Administrator);

        Result<AdminUserDto> result = await _sut.Handle(
            Command(Roles.SuperAdministrator), CancellationToken.None);

        result.Error.Should().Be(UserAdminErrors.CannotGrantSuperAdmin);
        await _users.DidNotReceive().CreateAccountAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_super_administrator_may_add_another_super_administrator()
    {
        SignedInAs(Roles.SuperAdministrator);
        _directory.GetAsync(_newUserId, Arg.Any<CancellationToken>())
            .Returns(UserAdminTestData.User(_newUserId, Roles.SuperAdministrator));

        Result<AdminUserDto> result = await _sut.Handle(
            Command(Roles.SuperAdministrator), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task A_duplicate_email_is_surfaced_as_a_conflict()
    {
        SignedInAs(Roles.Administrator);
        _users.CreateAccountAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Guid>(UserAdminErrors.EmailInUse));

        Result<AdminUserDto> result = await _sut.Handle(Command(), CancellationToken.None);

        result.Error.Should().Be(UserAdminErrors.EmailInUse);
        await _auditLogger.DidNotReceive().RecordAsync(
            Arg.Any<Guid>(), Arg.Any<AuditCategory>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }
}
