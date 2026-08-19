using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Payments.Common;
using NovaLearn.Domain.Payments;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Payments.GetTransactions;

/// <summary>Paged, filtered search over the finance ledger. All filters are optional.</summary>
public sealed record GetTransactionsQuery(
    PaymentStatus? Status,
    string? Search,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    int Page,
    int PageSize) : IRequest<Result<PagedResult<TransactionDto>>>;

public sealed class GetTransactionsQueryHandler(IPaymentRepository payments, ICurrentUser currentUser)
    : IRequestHandler<GetTransactionsQuery, Result<PagedResult<TransactionDto>>>
{
    public async Task<Result<PagedResult<TransactionDto>>> Handle(
        GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        if (!PaymentAuthority.IsAdmin(currentUser))
        {
            return Result.Failure<PagedResult<TransactionDto>>(PaymentErrors.Forbidden);
        }

        PagedResult<TransactionDto> result = await payments.ListTransactionsAsync(
            request.Status,
            request.Search,
            request.FromUtc,
            request.ToUtc,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result.Success(result);
    }
}
