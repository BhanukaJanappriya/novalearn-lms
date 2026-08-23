using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Payments.Common;
using NovaLearn.Application.Features.Reports.Common;
using NovaLearn.Application.Features.Reports.GetRevenueReport;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Payments;
using NovaLearn.Domain.Reports;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Reports;

public sealed class GetRevenueReportQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 23, 12, 0, 0, TimeSpan.Zero);

    private readonly IPaymentRepository _payments = Substitute.For<IPaymentRepository>();
    private readonly IReportRunRepository _reportRuns = Substitute.For<IReportRunRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly GetRevenueReportQueryHandler _sut;
    private readonly Guid _staffId = Guid.NewGuid();

    public GetRevenueReportQueryHandlerTests()
    {
        _sut = new GetRevenueReportQueryHandler(_payments, _reportRuns, _currentUser, _clock, _unitOfWork);
        _clock.UtcNow.Returns(Now);
        _currentUser.UserId.Returns(_staffId);
        _currentUser.IsInRole(Roles.Administrator).Returns(true);
    }

    [Fact]
    public async Task A_non_staff_caller_is_refused()
    {
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);

        Result<IReadOnlyList<TransactionDto>> result =
            await _sut.Handle(new GetRevenueReportQuery(null, null, null), CancellationToken.None);

        result.Error.Should().Be(ReportErrors.StaffOnly);
    }

    [Fact]
    public async Task A_successful_run_asks_for_the_export_page_size_and_logs_itself()
    {
        var transaction = new TransactionDto(
            Guid.NewGuid(), Guid.NewGuid(), "Jane Learner", "jane@example.com", Guid.NewGuid(),
            "Intro to Programming", 100m, "usd", PaymentStatus.Succeeded, null, Now, Now, null, null);

        _payments.ListTransactionsAsync(
                PaymentStatus.Succeeded, null, Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(),
                1, ReportExport.MaxRows, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<TransactionDto>([transaction], 1, ReportExport.MaxRows, 1));

        Result<IReadOnlyList<TransactionDto>> result = await _sut.Handle(
            new GetRevenueReportQuery(PaymentStatus.Succeeded, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle().Which.Should().Be(transaction);

        await _reportRuns.Received(1).AddAsync(
            Arg.Is<ReportRun>(r => r.Type == ReportType.Revenue && r.RowCount == 1),
            Arg.Any<CancellationToken>());
    }
}
