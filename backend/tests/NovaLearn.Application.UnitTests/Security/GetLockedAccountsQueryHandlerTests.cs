using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Security.Common;
using NovaLearn.Application.Features.Security.GetLockedAccounts;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Security;

public sealed class GetLockedAccountsQueryHandlerTests
{
    private readonly ISecurityRepository _security = Substitute.For<ISecurityRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly GetLockedAccountsQueryHandler _sut;

    public GetLockedAccountsQueryHandlerTests()
    {
        _sut = new GetLockedAccountsQueryHandler(_security, _currentUser);
    }

    private void SignedInAs(params string[] roles) =>
        _currentUser.IsInRole(Arg.Any<string>()).Returns(call => roles.Contains(call.Arg<string>()));

    [Fact]
    public async Task A_non_staff_caller_is_refused()
    {
        SignedInAs(Roles.Student);

        Result<PagedResult<LockedAccountRow>> result = await _sut.Handle(
            new GetLockedAccountsQuery(null, 1, 20), CancellationToken.None);

        result.Error.Should().Be(SecurityErrors.StaffOnly);
    }

    [Fact]
    public async Task A_staff_caller_gets_the_filtered_page()
    {
        SignedInAs(Roles.Administrator);

        var row = new LockedAccountRow(
            Guid.NewGuid(), "Jane Learner", "jane@example.com", DateTimeOffset.UtcNow.AddMinutes(10), 5);

        _security.ListLockedAccountsAsync("jane", 1, 20, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<LockedAccountRow>([row], 1, 20, 1));

        Result<PagedResult<LockedAccountRow>> result = await _sut.Handle(
            new GetLockedAccountsQuery("jane", 1, 20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle().Which.Should().Be(row);
    }
}
