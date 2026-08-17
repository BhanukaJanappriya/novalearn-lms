using FluentAssertions;
using NSubstitute;
using NSubstitute.Core;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Admin.Analytics;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Admin;

public sealed class TrendMetricTests
{
    [Theory]
    [InlineData(120, 100, 20)]
    [InlineData(80, 100, -20)]
    [InlineData(100, 100, 0)]
    [InlineData(3, 8, -62.5)]
    public void The_change_is_measured_against_the_previous_window(
        double current, double previous, double expected)
    {
        new TrendMetric(current, previous).ChangePercent.Should().Be(expected);
    }

    [Theory]
    [InlineData(50, 0)]
    [InlineData(0, 0)]
    public void There_is_no_change_to_report_when_the_previous_window_was_empty(
        double current, double previous)
    {
        // Everything is an infinite improvement on nothing. Saying so plainly beats rendering a
        // triumphant number the first time a metric is ever recorded.
        new TrendMetric(current, previous).ChangePercent.Should().BeNull();
    }

    [Fact]
    public void A_drop_to_zero_is_reported_as_a_full_loss_rather_than_as_nothing()
    {
        new TrendMetric(0, 40).ChangePercent.Should().Be(-100);
    }
}

public sealed class GetPlatformAnalyticsQueryHandlerTests
{
    private readonly IPlatformAnalytics _analytics = Substitute.For<IPlatformAnalytics>();
    private readonly GetPlatformAnalyticsQueryHandler _sut;

    public GetPlatformAnalyticsQueryHandlerTests()
    {
        _sut = new GetPlatformAnalyticsQueryHandler(_analytics);

        _analytics.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Empty());
    }

    private static PlatformAnalytics Empty() =>
        new(
            new AnalyticsWindow(DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow, 30,
                AnalyticsGranularity.Day),
            new AnalyticsHeadline(
                new TrendMetric(0, 0), new TrendMetric(0, 0),
                new TrendMetric(0, 0), new TrendMetric(0, 0)),
            [], [], [], []);

    /// <summary>The window length the handler actually asked the read model for.</summary>
    private async Task<int> DaysAskedFor(int requested)
    {
        await _sut.Handle(new GetPlatformAnalyticsQuery(requested), CancellationToken.None);

        return _analytics.ReceivedCalls()
            .Select(call => call.GetArguments())
            .Select(arguments => (int)arguments[0]!)
            .Last();
    }

    [Theory]
    [InlineData(7)]
    [InlineData(30)]
    [InlineData(90)]
    [InlineData(365)]
    public async Task A_window_the_picker_offers_is_passed_through_untouched(int days)
    {
        (await DaysAskedFor(days)).Should().Be(days);
    }

    [Theory]
    [InlineData(0, 7)]
    [InlineData(-100, 7)]
    [InlineData(1, 7)]
    [InlineData(100_000, 365)]
    public async Task An_out_of_range_window_is_clamped_rather_than_refused(int requested, int expected)
    {
        // A hand written request asking for ten years of daily buckets gets the nearest sensible
        // window instead of an error, and cannot make the server build an enormous series.
        (await DaysAskedFor(requested)).Should().Be(expected);
    }

    [Fact]
    public async Task The_result_is_always_a_success_since_an_empty_platform_is_not_a_failure()
    {
        Result<PlatformAnalytics> result =
            await _sut.Handle(new GetPlatformAnalyticsQuery(30), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Series.Should().BeEmpty();
    }
}
