using NovaLearn.Domain.Payments;

namespace NovaLearn.Application.Features.Payments.Common;

/// <summary>Where to send the student to pay.</summary>
public sealed record CheckoutSessionDto(string CheckoutUrl);

/// <summary>
/// One payment as an administrator sees it on the ledger.
///
/// Carries the student's name for the row but nothing else about their account: this is a
/// transaction list, not a second copy of user management.
/// </summary>
public sealed record TransactionDto(
    Guid Id,
    Guid StudentId,
    string StudentName,
    string StudentEmail,
    Guid CourseId,
    string CourseTitle,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    decimal? RefundedAmount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? PaidAtUtc,
    DateTimeOffset? RefundedAtUtc,
    string? FailureReason)
{
    /// <summary>Whether there is anything left on this payment for an admin to refund.</summary>
    public bool CanRefund =>
        Status is PaymentStatus.Succeeded or PaymentStatus.PartiallyRefunded
        && (RefundedAmount ?? 0) < Amount;

    /// <summary>What refunding the rest of this payment right now would give back.</summary>
    public decimal RefundableAmount => CanRefund ? Amount - (RefundedAmount ?? 0) : 0;
}

/// <summary>Maps the aggregate onto its ledger row.</summary>
public static class PaymentMapper
{
    public static TransactionDto ToTransactionDto(Payment payment) =>
        new(
            payment.Id,
            payment.StudentId,
            payment.Student is { } student ? $"{student.FirstName} {student.LastName}".Trim() : "Unknown",
            payment.Student?.Email ?? string.Empty,
            payment.CourseId,
            payment.CourseTitle,
            payment.Amount,
            payment.Currency,
            payment.Status,
            payment.RefundedAmount,
            payment.CreatedAtUtc,
            payment.PaidAtUtc,
            payment.RefundedAtUtc,
            payment.FailureReason);
}
