using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Admin.Dashboard;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Admin;

public sealed class GetAdminDashboardQueryHandlerTests
{
    private readonly IAdminStatisticsService _statistics = Substitute.For<IAdminStatisticsService>();
    private readonly IPlatformAnalytics _platformAnalytics = Substitute.For<IPlatformAnalytics>();
    private readonly GetAdminDashboardQueryHandler _sut;

    public GetAdminDashboardQueryHandlerTests()
    {
        _sut = new GetAdminDashboardQueryHandler(_statistics, _platformAnalytics);
        _statistics.GetStatisticsAsync(Arg.Any<CancellationToken>()).Returns(EmptyStatistics());
    }

    private static AdminStatistics EmptyStatistics() =>
        new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, [], [], [], [], []);

    private static PlatformAnalytics AnalyticsWith(
        AnalyticsGranularity granularity, params AnalyticsPoint[] series) =>
        new(
            new AnalyticsWindow(DateTimeOffset.UtcNow.AddDays(-7), DateTimeOffset.UtcNow, 7, granularity),
            new AnalyticsHeadline(
                new TrendMetric(0, 0), new TrendMetric(0, 0), new TrendMetric(0, 0), new TrendMetric(0, 0)),
            series,
            [],
            [],
            []);

    private Task<Result<AdminDashboardResponse>> Act(int days) =>
        _sut.Handle(new GetAdminDashboardQuery(days), CancellationToken.None);

    [Fact]
    public async Task The_requested_window_is_passed_straight_through_to_platform_analytics()
    {
        _platformAnalytics.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(AnalyticsWith(AnalyticsGranularity.Day));

        await Act(7);

        await _platformAnalytics.Received(1).GetAsync(7, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task The_enrollment_trend_carries_completions_as_its_comparison_series()
    {
        var point = new AnalyticsPoint(new DateOnly(2026, 8, 24), 12, 5);
        _platformAnalytics.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(AnalyticsWith(AnalyticsGranularity.Day, point));

        Result<AdminDashboardResponse> result = await Act(7);

        result.Value.EnrollmentTrend.Should().ContainSingle();
        result.Value.EnrollmentTrend[0].Value.Should().Be(12);
        result.Value.EnrollmentTrend[0].Compare.Should().Be(5);
        result.Value.EnrollmentTrend[0].Label.Should().Be("Aug 24");
    }

    [Fact]
    public async Task The_completion_trend_has_no_comparison_series()
    {
        var point = new AnalyticsPoint(new DateOnly(2026, 8, 24), 12, 5);
        _platformAnalytics.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(AnalyticsWith(AnalyticsGranularity.Day, point));

        Result<AdminDashboardResponse> result = await Act(7);

        result.Value.CompletionTrend.Should().ContainSingle();
        result.Value.CompletionTrend[0].Value.Should().Be(5);
        result.Value.CompletionTrend[0].Compare.Should().BeNull();
    }

    /// <summary>
    /// A short window buckets by day (see <c>AnalyticsBucketing.GranularityFor</c>), so each
    /// point needs the day on its label; a long window buckets by month, where the day would be
    /// meaningless since every point already represents a whole month.
    /// </summary>
    [Fact]
    public async Task A_month_bucketed_window_labels_points_with_just_the_month()
    {
        var point = new AnalyticsPoint(new DateOnly(2026, 3, 1), 40, 10);
        _platformAnalytics.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(AnalyticsWith(AnalyticsGranularity.Month, point));

        Result<AdminDashboardResponse> result = await Act(365);

        result.Value.EnrollmentTrend[0].Label.Should().Be("Mar");
    }
}
