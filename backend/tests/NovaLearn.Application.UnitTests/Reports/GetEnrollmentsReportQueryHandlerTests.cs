using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Reports.Common;
using NovaLearn.Application.Features.Reports.GetEnrollmentsReport;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Reports;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Reports;

public sealed class GetEnrollmentsReportQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 23, 12, 0, 0, TimeSpan.Zero);

    private readonly IReportsRepository _reports = Substitute.For<IReportsRepository>();
    private readonly IReportRunRepository _reportRuns = Substitute.For<IReportRunRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly GetEnrollmentsReportQueryHandler _sut;
    private readonly Guid _staffId = Guid.NewGuid();

    public GetEnrollmentsReportQueryHandlerTests()
    {
        _sut = new GetEnrollmentsReportQueryHandler(_reports, _reportRuns, _currentUser, _clock, _unitOfWork);
        _clock.UtcNow.Returns(Now);
        _currentUser.UserId.Returns(_staffId);
        SignedInAs(Roles.Administrator);
    }

    private void SignedInAs(params string[] roles) =>
        _currentUser.IsInRole(Arg.Any<string>()).Returns(call => roles.Contains(call.Arg<string>()));

    [Fact]
    public async Task A_non_staff_caller_is_refused()
    {
        SignedInAs(Roles.Student);

        Result<IReadOnlyList<EnrollmentReportRow>> result =
            await _sut.Handle(new GetEnrollmentsReportQuery(null, null, null), CancellationToken.None);

        result.Error.Should().Be(ReportErrors.StaffOnly);
        await _reports.DidNotReceive().ListEnrollmentsAsync(
            Arg.Any<EnrollmentStatus?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_successful_run_returns_the_rows_and_logs_itself()
    {
        var rows = new List<EnrollmentReportRow>
        {
            new(
                Guid.NewGuid(), Guid.NewGuid(), "Jane Learner", "jane@example.com",
                Guid.NewGuid(), "Intro to Programming", EnrollmentStatus.Active, 40, Now, null)
        };

        _reports.ListEnrollmentsAsync(
                EnrollmentStatus.Active, Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(),
                Arg.Any<CancellationToken>())
            .Returns(rows);

        Result<IReadOnlyList<EnrollmentReportRow>> result = await _sut.Handle(
            new GetEnrollmentsReportQuery(EnrollmentStatus.Active, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(rows);

        await _reportRuns.Received(1).AddAsync(
            Arg.Is<ReportRun>(r =>
                r.Type == ReportType.Enrollments && r.GeneratedById == _staffId && r.RowCount == 1),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
