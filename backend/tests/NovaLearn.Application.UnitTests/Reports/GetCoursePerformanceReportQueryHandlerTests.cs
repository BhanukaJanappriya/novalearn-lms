using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Reports.GetCoursePerformanceReport;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Reports;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Reports;

public sealed class GetCoursePerformanceReportQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 23, 12, 0, 0, TimeSpan.Zero);

    private readonly IPlatformAnalytics _platformAnalytics = Substitute.For<IPlatformAnalytics>();
    private readonly IReportRunRepository _reportRuns = Substitute.For<IReportRunRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly GetCoursePerformanceReportQueryHandler _sut;
    private readonly Guid _staffId = Guid.NewGuid();

    public GetCoursePerformanceReportQueryHandlerTests()
    {
        _sut = new GetCoursePerformanceReportQueryHandler(
            _platformAnalytics, _reportRuns, _currentUser, _clock, _unitOfWork);
        _clock.UtcNow.Returns(Now);
        _currentUser.UserId.Returns(_staffId);
        _currentUser.IsInRole(Roles.Administrator).Returns(true);
    }

    private static PlatformAnalytics AnalyticsWith(params CoursePerformanceRow[] courses) =>
        new(
            new AnalyticsWindow(Now.AddDays(-30), Now, 30, AnalyticsGranularity.Day),
            new AnalyticsHeadline(
                new TrendMetric(0, 0), new TrendMetric(0, 0), new TrendMetric(0, 0), new TrendMetric(0, 0)),
            [],
            courses,
            [],
            []);

    [Fact]
    public async Task A_non_staff_caller_is_refused()
    {
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);

        Result<IReadOnlyList<CoursePerformanceRow>> result =
            await _sut.Handle(new GetCoursePerformanceReportQuery(), CancellationToken.None);

        result.Error.Should().Be(ReportErrors.StaffOnly);
    }

    [Fact]
    public async Task A_successful_run_returns_the_lifetime_course_rows_and_logs_itself()
    {
        var course = new CoursePerformanceRow(
            Guid.NewGuid(), "Intro to Programming", null, null, 10, 4, 40.0, 55.0, 72.5, 1);

        _platformAnalytics.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(AnalyticsWith(course));

        Result<IReadOnlyList<CoursePerformanceRow>> result =
            await _sut.Handle(new GetCoursePerformanceReportQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle().Which.Should().Be(course);

        await _reportRuns.Received(1).AddAsync(
            Arg.Is<ReportRun>(r => r.Type == ReportType.CoursePerformance && r.RowCount == 1),
            Arg.Any<CancellationToken>());
    }
}
