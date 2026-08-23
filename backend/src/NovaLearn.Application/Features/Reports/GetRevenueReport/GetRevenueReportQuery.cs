using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Payments.Common;
using NovaLearn.Application.Features.Reports.Common;
using NovaLearn.Domain.Payments;
using NovaLearn.Domain.Reports;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Reports.GetRevenueReport;

/// <summary>
/// Every payment, for export. Reuses the finance ledger's own paged query with a large page size
/// in place of a true unlimited one — see the remarks on <see cref="IReportsRepository"/>. Staff only.
/// </summary>
public sealed record GetRevenueReportQuery(
    PaymentStatus? Status, DateTimeOffset? FromUtc, DateTimeOffset? ToUtc)
    : IRequest<Result<IReadOnlyList<TransactionDto>>>;

public sealed class GetRevenueReportQueryHandler(
    IPaymentRepository payments,
    IReportRunRepository reportRuns,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : IRequestHandler<GetRevenueReportQuery, Result<IReadOnlyList<TransactionDto>>>
{
    public async Task<Result<IReadOnlyList<TransactionDto>>> Handle(
        GetRevenueReportQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId || !ReportAuthority.IsStaff(currentUser))
        {
            return Result.Failure<IReadOnlyList<TransactionDto>>(ReportErrors.StaffOnly);
        }

        PagedResult<TransactionDto> page = await payments.ListTransactionsAsync(
            request.Status,
            search: null,
            request.FromUtc,
            request.ToUtc,
            page: 1,
            ReportExport.MaxRows,
            cancellationToken);

        string? filters = ReportFilters.Summarize(
            ("status", request.Status), ("from", request.FromUtc), ("to", request.ToUtc));

        await reportRuns.AddAsync(
            ReportRun.Create(ReportType.Revenue, callerId, filters, page.Items.Count, dateTimeProvider.UtcNow),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success<IReadOnlyList<TransactionDto>>(page.Items);
    }
}
