using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.AuditLogs.Common;
using NovaLearn.Application.Features.AuditLogs.GetAuditLogs;
using NovaLearn.Domain.Audit;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Audit;

public sealed class GetAuditLogsQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 26, 12, 0, 0, TimeSpan.Zero);

    private readonly IAuditLogRepository _auditLogs = Substitute.For<IAuditLogRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly GetAuditLogsQueryHandler _sut;

    public GetAuditLogsQueryHandlerTests()
    {
        _sut = new GetAuditLogsQueryHandler(_auditLogs, _currentUser);
    }

    private void SignedInAs(params string[] roles) =>
        _currentUser.IsInRole(Arg.Any<string>()).Returns(call => roles.Contains(call.Arg<string>()));

    [Fact]
    public async Task A_non_staff_caller_is_refused()
    {
        SignedInAs(Roles.Student);

        Result<PagedResult<AuditLogRow>> result = await _sut.Handle(
            new GetAuditLogsQuery(null, null, null, null, null, 1, 20), CancellationToken.None);

        result.Error.Should().Be(AuditErrors.StaffOnly);
        await _auditLogs.DidNotReceive().SearchAsync(
            Arg.Any<AuditCategory?>(), Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<DateTimeOffset?>(),
            Arg.Any<DateTimeOffset?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_staff_caller_gets_the_filtered_page()
    {
        SignedInAs(Roles.Administrator);

        var row = new AuditLogRow(
            Guid.NewGuid(), AuditCategory.Finance, "Refunded payment", "80 usd", "Payment", Guid.NewGuid(),
            Guid.NewGuid(), "Nova Administrator", "admin@novalearn.local", Now);

        _auditLogs.SearchAsync(
                AuditCategory.Finance, null, null, Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(),
                1, 20, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<AuditLogRow>([row], 1, 20, 1));

        Result<PagedResult<AuditLogRow>> result = await _sut.Handle(
            new GetAuditLogsQuery(AuditCategory.Finance, null, null, null, null, 1, 20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle().Which.Should().Be(row);
    }
}
