using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Payments.Common;
using NovaLearn.Domain.Audit;
using NovaLearn.Domain.Payments;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Payments.RefundPayment;

/// <summary>
/// Refunds a payment, in whole or in part. <see cref="Amount"/> null means everything still left
/// on it.
/// </summary>
public sealed record RefundPaymentCommand(Guid PaymentId, decimal? Amount) : IRequest<Result<TransactionDto>>;

/// <summary>
/// Carries out a refund.
///
/// The gateway is called before anything here is written, since the gateway's word is the one
/// that is actually true: if it fails, the ledger has nothing to undo. If persisting the result
/// afterwards fails, the refund already happened at the gateway and the operator is told plainly
/// to check, rather than being handed a bare five-hundred that suggests nothing happened at all.
///
/// Not fully immune to two admins (or two clicks) refunding the same payment at once: both could
/// read it as refundable before either writes, and both would then call the gateway. That would
/// need a lock taken before the gateway call to close, which is more machinery than this ledger's
/// real usage pattern — one admin, occasional refunds — currently justifies. Left as a known,
/// documented gap rather than quietly assumed away.
/// </summary>
public sealed class RefundPaymentCommandHandler(
    IPaymentRepository payments,
    IPaymentGateway gateway,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IAuditLogger auditLogger)
    : IRequestHandler<RefundPaymentCommand, Result<TransactionDto>>
{
    public async Task<Result<TransactionDto>> Handle(
        RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        if (!PaymentAuthority.IsAdmin(currentUser))
        {
            return Result.Failure<TransactionDto>(PaymentErrors.Forbidden);
        }

        Payment? payment = await payments.GetByIdAsync(request.PaymentId, cancellationToken);
        if (payment is null)
        {
            return Result.Failure<TransactionDto>(PaymentErrors.NotFound);
        }

        if (payment.Status is not (PaymentStatus.Succeeded or PaymentStatus.PartiallyRefunded))
        {
            return Result.Failure<TransactionDto>(PaymentErrors.NotRefundable);
        }

        decimal remaining = payment.Amount - (payment.RefundedAmount ?? 0);
        decimal amount = request.Amount ?? remaining;

        if (amount <= 0)
        {
            return Result.Failure<TransactionDto>(PaymentErrors.RefundAmountMustBePositive);
        }

        if (amount > remaining)
        {
            return Result.Failure<TransactionDto>(PaymentErrors.RefundExceedsRemaining);
        }

        try
        {
            await gateway.RefundAsync(
                payment.ProviderPaymentIntentId!, amount, payment.Currency, cancellationToken);
        }
        catch (Exception exception)
        {
            return Result.Failure<TransactionDto>(PaymentErrors.GatewayError(exception.Message));
        }

        payment.RecordRefund(amount, dateTimeProvider.UtcNow);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            // The refund already happened at the gateway by this point; a failure to persist it
            // is a "check the ledger" situation, not a "the refund did not happen" one.
            return Result.Failure<TransactionDto>(PaymentErrors.ConcurrentModification);
        }

        await auditLogger.RecordAsync(
            currentUser.UserId!.Value,
            AuditCategory.Finance,
            "Refunded payment",
            $"{amount} {payment.Currency} for {payment.CourseTitle}",
            "Payment",
            payment.Id,
            cancellationToken);

        return Result.Success(PaymentMapper.ToTransactionDto(payment));
    }
}
