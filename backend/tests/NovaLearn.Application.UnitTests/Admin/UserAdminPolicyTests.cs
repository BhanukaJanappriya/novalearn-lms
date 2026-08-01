using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Admin.Users.Common;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Admin;

/// <summary>
/// The authority rules around account administration. These are the checks that stop an
/// administrator locking themselves out, escalating their own privileges, or stranding the
/// platform with no super administrator.
/// </summary>
public sealed class UserAdminPolicyTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly Guid _callerId = Guid.NewGuid();
    private readonly Guid _targetId = Guid.NewGuid();

    private void SignedInAs(Guid id, params string[] roles)
    {
        _currentUser.UserId.Returns(id);
        _currentUser.IsInRole(Arg.Any<string>()).Returns(call => roles.Contains(call.Arg<string>()));
    }

    // --- CheckCanModify -----------------------------------------------------------------

    [Fact]
    public void An_administrator_cannot_act_on_their_own_account()
    {
        SignedInAs(_callerId, Roles.Administrator);
        AdminUserRow target = UserAdminTestData.User(_callerId, Roles.Administrator);

        UserAdminPolicy.CheckCanModify(target, _currentUser)
            .Should().Be(UserAdminErrors.CannotModifySelf);
    }

    [Fact]
    public void A_super_administrator_cannot_act_on_their_own_account_either()
    {
        SignedInAs(_callerId, Roles.SuperAdministrator);
        AdminUserRow target = UserAdminTestData.User(_callerId, Roles.SuperAdministrator);

        UserAdminPolicy.CheckCanModify(target, _currentUser)
            .Should().Be(UserAdminErrors.CannotModifySelf);
    }

    [Fact]
    public void An_ordinary_administrator_cannot_act_on_a_super_administrator()
    {
        SignedInAs(_callerId, Roles.Administrator);
        AdminUserRow target = UserAdminTestData.User(_targetId, Roles.SuperAdministrator);

        UserAdminPolicy.CheckCanModify(target, _currentUser)
            .Should().Be(UserAdminErrors.SuperAdminOnly);
    }

    [Fact]
    public void A_super_administrator_may_act_on_another_super_administrator()
    {
        SignedInAs(_callerId, Roles.SuperAdministrator);
        AdminUserRow target = UserAdminTestData.User(_targetId, Roles.SuperAdministrator);

        UserAdminPolicy.CheckCanModify(target, _currentUser).Should().BeNull();
    }

    [Fact]
    public void An_administrator_may_act_on_an_ordinary_account()
    {
        SignedInAs(_callerId, Roles.Administrator);
        AdminUserRow target = UserAdminTestData.User(_targetId, Roles.Student);

        UserAdminPolicy.CheckCanModify(target, _currentUser).Should().BeNull();
    }

    [Fact]
    public void An_unauthenticated_caller_is_rejected()
    {
        _currentUser.UserId.Returns((Guid?)null);
        AdminUserRow target = UserAdminTestData.User(_targetId, Roles.Student);

        UserAdminPolicy.CheckCanModify(target, _currentUser)
            .Should().Be(UserAdminErrors.Unauthenticated);
    }

    // --- CheckRoleAssignment ------------------------------------------------------------

    [Fact]
    public void An_unknown_role_name_is_rejected()
    {
        SignedInAs(_callerId, Roles.SuperAdministrator);
        AdminUserRow target = UserAdminTestData.User(_targetId, Roles.Student);

        Error? error = UserAdminPolicy.CheckRoleAssignment(target, ["Wizard"], _currentUser);

        error.Should().NotBeNull();
        error!.Code.Should().Be("user_admin.unknown_role");
    }

    [Fact]
    public void An_ordinary_administrator_cannot_grant_the_super_administrator_role()
    {
        SignedInAs(_callerId, Roles.Administrator);
        AdminUserRow target = UserAdminTestData.User(_targetId, Roles.Student);

        UserAdminPolicy.CheckRoleAssignment(target, [Roles.SuperAdministrator], _currentUser)
            .Should().Be(UserAdminErrors.CannotGrantSuperAdmin);
    }

    [Fact]
    public void An_ordinary_administrator_cannot_revoke_the_super_administrator_role()
    {
        SignedInAs(_callerId, Roles.Administrator);
        AdminUserRow target = UserAdminTestData.User(_targetId, Roles.SuperAdministrator);

        UserAdminPolicy.CheckRoleAssignment(target, [Roles.Administrator], _currentUser)
            .Should().Be(UserAdminErrors.CannotGrantSuperAdmin);
    }

    [Fact]
    public void An_administrator_may_reshuffle_roles_that_leave_super_administrator_untouched()
    {
        SignedInAs(_callerId, Roles.Administrator);
        AdminUserRow target = UserAdminTestData.User(_targetId, Roles.Student);

        UserAdminPolicy
            .CheckRoleAssignment(target, [Roles.Student, Roles.TeachingAssistant], _currentUser)
            .Should().BeNull();
    }

    [Fact]
    public void A_super_administrator_keeping_their_own_role_set_is_not_treated_as_a_change()
    {
        SignedInAs(_callerId, Roles.Administrator);
        AdminUserRow target = UserAdminTestData.User(_targetId, Roles.SuperAdministrator, Roles.Lecturer);

        // Super administrator is present before and after, so an ordinary admin may edit the rest.
        UserAdminPolicy
            .CheckRoleAssignment(target, [Roles.SuperAdministrator, Roles.Student], _currentUser)
            .Should().BeNull();
    }

    // --- CheckNotStrandingPlatform ------------------------------------------------------

    [Fact]
    public void The_last_super_administrator_cannot_lose_the_role()
    {
        AdminUserRow target = UserAdminTestData.User(_targetId, Roles.SuperAdministrator);

        UserAdminPolicy.CheckNotStrandingPlatform(target, retainsSuperAdmin: false, superAdminCount: 1)
            .Should().Be(UserAdminErrors.LastSuperAdmin);
    }

    [Fact]
    public void A_super_administrator_may_lose_the_role_while_others_remain()
    {
        AdminUserRow target = UserAdminTestData.User(_targetId, Roles.SuperAdministrator);

        UserAdminPolicy.CheckNotStrandingPlatform(target, retainsSuperAdmin: false, superAdminCount: 2)
            .Should().BeNull();
    }

    [Fact]
    public void Keeping_the_role_is_always_allowed()
    {
        AdminUserRow target = UserAdminTestData.User(_targetId, Roles.SuperAdministrator);

        UserAdminPolicy.CheckNotStrandingPlatform(target, retainsSuperAdmin: true, superAdminCount: 1)
            .Should().BeNull();
    }

    [Fact]
    public void An_account_that_was_never_a_super_administrator_is_unaffected()
    {
        AdminUserRow target = UserAdminTestData.User(_targetId, Roles.Student);

        UserAdminPolicy.CheckNotStrandingPlatform(target, retainsSuperAdmin: false, superAdminCount: 1)
            .Should().BeNull();
    }
}
