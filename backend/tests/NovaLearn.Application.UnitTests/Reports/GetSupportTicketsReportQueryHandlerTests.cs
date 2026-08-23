using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Reports.Common;
using NovaLearn.Application.Features.Reports.GetSupportTicketsReport;
using NovaLearn.Application.Features.Support.Common;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Reports;
using NovaLearn.Domain.Support;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Reports;

public sealed class GetSupportTicketsReportQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 23, 12, 0, 0, TimeSpan.Zero);

    private readonly ISupportTicketRepository _tickets = Substitute.For<ISupportTicketRepository>();
    private readonly IReportRunRepository _reportRuns = Substitute.For<IReportRunRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly GetSupportTicketsReportQueryHandler _sut;
    private readonly Guid _staffId = Guid.NewGuid();

    public GetSupportTicketsReportQueryHandlerTests()
    {
        _sut = new GetSupportTicketsReportQueryHandler(_tickets, _reportRuns, _currentUser, _clock, _unitOfWork);
        _clock.UtcNow.Returns(Now);
        _currentUser.UserId.Returns(_staffId);
        _currentUser.IsInRole(Roles.Administrator).Returns(true);
    }

    [Fact]
    public async Task A_non_staff_caller_is_refused()
    {
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);

        Result<IReadOnlyList<TicketSummaryDto>> result = await _sut.Handle(
            new GetSupportTicketsReportQuery(null, null, null), CancellationToken.None);

        result.Error.Should().Be(ReportErrors.StaffOnly);
    }

    [Fact]
    public async Task A_successful_run_maps_tickets_to_summary_rows_and_logs_itself()
    {
        SupportTicket ticket = SupportTicket.Create(
            Guid.NewGuid(), "Cannot access my course", TicketCategory.Technical, TicketPriority.Normal,
            "The video player never loads.", Now);

        _tickets.ListForStaffAsync(
                TicketStatus.Open, null, null, null, null, 1, ReportExport.MaxRows, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<SupportTicket>([ticket], 1, ReportExport.MaxRows, 1));

        Result<IReadOnlyList<TicketSummaryDto>> result = await _sut.Handle(
            new GetSupportTicketsReportQuery(TicketStatus.Open, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle().Which.Id.Should().Be(ticket.Id);

        await _reportRuns.Received(1).AddAsync(
            Arg.Is<ReportRun>(r => r.Type == ReportType.SupportTickets && r.RowCount == 1),
            Arg.Any<CancellationToken>());
    }
}
