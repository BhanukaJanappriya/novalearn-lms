using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Security.Common;
using NovaLearn.Application.Features.Security.GetSecurityOverview;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Security;

public sealed class GetSecurityOverviewQueryHandlerTests
{
    private readonly ISecurityRepository _security = Substitute.For<ISecurityRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly GetSecurityOverviewQueryHandler _sut;

    public GetSecurityOverviewQueryHandlerTests()
    {
        _sut = new GetSecurityOverviewQueryHandler(_security, _currentUser);
    }

    private void SignedInAs(params string[] roles) =>
        _currentUser.IsInRole(Arg.Any<string>()).Returns(call => roles.Contains(call.Arg<string>()));

    [Fact]
    public async Task A_non_staff_caller_is_refused()
    {
        SignedInAs(Roles.Student);

        Result<SecurityOverview> result = await _sut.Handle(new GetSecurityOverviewQuery(), CancellationToken.None);

        result.Error.Should().Be(SecurityErrors.StaffOnly);
        await _security.DidNotReceive().GetOverviewAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_staff_caller_gets_the_overview()
    {
        SignedInAs(Roles.Administrator);
        var overview = new SecurityOverview(4, 1, 12, 33.3);
        _security.GetOverviewAsync(Arg.Any<CancellationToken>()).Returns(overview);

        Result<SecurityOverview> result = await _sut.Handle(new GetSecurityOverviewQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(overview);
    }
}
