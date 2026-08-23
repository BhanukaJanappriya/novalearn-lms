using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Reports.Common;
using NovaLearn.Application.Features.Reports.GetUsersReport;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Reports;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Reports;

public sealed class GetUsersReportQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 23, 12, 0, 0, TimeSpan.Zero);

    private readonly IUserDirectory _users = Substitute.For<IUserDirectory>();
    private readonly IReportRunRepository _reportRuns = Substitute.For<IReportRunRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly GetUsersReportQueryHandler _sut;
    private readonly Guid _staffId = Guid.NewGuid();

    public GetUsersReportQueryHandlerTests()
    {
        _sut = new GetUsersReportQueryHandler(_users, _reportRuns, _currentUser, _clock, _unitOfWork);
        _clock.UtcNow.Returns(Now);
        _currentUser.UserId.Returns(_staffId);
        _currentUser.IsInRole(Roles.Administrator).Returns(true);
    }

    private static AdminUserRow Row() =>
        new(
            Guid.NewGuid(), "Jane", "Learner", "jane@example.com", null, true, true, false, Now, Now,
            ["Student"], 2, 0);

    [Fact]
    public async Task A_non_staff_caller_is_refused()
    {
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);

        Result<IReadOnlyList<AdminUserRow>> result =
            await _sut.Handle(new GetUsersReportQuery(null, null, null, null), CancellationToken.None);

        result.Error.Should().Be(ReportErrors.StaffOnly);
    }

    [Fact]
    public async Task A_successful_run_asks_for_the_export_page_size_and_logs_itself()
    {
        AdminUserRow row = Row();

        _users.SearchAsync(
                "jane", "Student", true, null, 1, ReportExport.MaxRows, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<AdminUserRow>([row], 1, ReportExport.MaxRows, 1));

        Result<IReadOnlyList<AdminUserRow>> result = await _sut.Handle(
            new GetUsersReportQuery("jane", "Student", true, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle().Which.Should().Be(row);

        await _reportRuns.Received(1).AddAsync(
            Arg.Is<ReportRun>(r => r.Type == ReportType.Users && r.RowCount == 1),
            Arg.Any<CancellationToken>());
    }
}
