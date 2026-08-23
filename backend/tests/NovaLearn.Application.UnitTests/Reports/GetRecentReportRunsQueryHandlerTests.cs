using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Reports.Common;
using NovaLearn.Application.Features.Reports.GetRecentReportRuns;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Reports;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Reports;

public sealed class GetRecentReportRunsQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 23, 12, 0, 0, TimeSpan.Zero);

    private readonly IReportRunRepository _reportRuns = Substitute.For<IReportRunRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly GetRecentReportRunsQueryHandler _sut;

    public GetRecentReportRunsQueryHandlerTests()
    {
        _sut = new GetRecentReportRunsQueryHandler(_reportRuns, _currentUser);
        _currentUser.IsInRole(Roles.Administrator).Returns(true);
    }

    [Fact]
    public async Task A_non_staff_caller_is_refused()
    {
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);

        Result<IReadOnlyList<ReportRunDto>> result =
            await _sut.Handle(new GetRecentReportRunsQuery(20), CancellationToken.None);

        result.Error.Should().Be(ReportErrors.StaffOnly);
    }

    [Fact]
    public async Task A_successful_call_maps_runs_newest_first()
    {
        ReportRun run = ReportRun.Create(ReportType.Revenue, Guid.NewGuid(), "status=Succeeded", 12, Now);

        _reportRuns.ListRecentAsync(20, Arg.Any<CancellationToken>()).Returns([run]);

        Result<IReadOnlyList<ReportRunDto>> result =
            await _sut.Handle(new GetRecentReportRunsQuery(20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value[0].Type.Should().Be(ReportType.Revenue);
        result.Value[0].RowCount.Should().Be(12);
        result.Value[0].GeneratedByName.Should().Be("Unknown");
    }
}
