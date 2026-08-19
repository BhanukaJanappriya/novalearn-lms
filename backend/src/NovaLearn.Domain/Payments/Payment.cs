using NovaLearn.Domain.Common;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Payments.Events;

namespace NovaLearn.Domain.Payments;

/// <summary>
/// One attempt to pay for a course, from checkout through to settlement and any refund.
///
/// <see cref="CourseTitle"/> and <see cref="Amount"/> are snapshotted at checkout rather than
/// read live off the course. A receipt has to keep saying what was actually bought and for how
/// much even if the course is later renamed or repriced; joining live would let history change
/// quietly underneath it.
///
/// Money moves in one direction through <see cref="Status"/>: Pending leads to Succeeded, Failed
/// or Expired, and only Succeeded can lead to PartiallyRefunded or Refunded. Nothing here calls
/// Stripe — this aggregate only records what the gateway already decided, so the same webhook
/// arriving twice (Stripe's delivery is at-least-once) settles into the same state rather than
/// refunding twice or double-booking a transition.
/// </summary>
public sealed class Payment : BaseEntity
{
    private Payment() { } // EF Core

    public Guid StudentId { get; private set; }

    public Guid CourseId { get; private set; }

    /// <summary>The course's title at the moment of checkout. See the class remarks.</summary>
    public string CourseTitle { get; private set; } = null!;

    /// <summary>The price at the moment of checkout, in major currency units (e.g. dollars, not cents).</summary>
    public decimal Amount { get; private set; }

    /// <summary>ISO 4217 currency code, lower case, e.g. "usd" — Stripe's own convention.</summary>
    public string Currency { get; private set; } = null!;

    public PaymentStatus Status { get; private set; }

    /// <summary>Stripe's id for the Checkout Session, set from the moment checkout starts.</summary>
    public string ProviderCheckoutSessionId { get; private set; } = null!;

    /// <summary>Stripe's id for the underlying payment, set once it succeeds.</summary>
    public string? ProviderPaymentIntentId { get; private set; }

    /// <summary>The enrolment this payment unlocked, set in the same operation that marks it paid.</summary>
    public Guid? EnrollmentId { get; private set; }

    /// <summary>Total refunded so far. Null until at least one refund has been recorded.</summary>
    public decimal? RefundedAmount { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTimeOffset? PaidAtUtc { get; private set; }

    public DateTimeOffset? RefundedAtUtc { get; private set; }

    public ApplicationUser? Student { get; private set; }

    public Course? Course { get; private set; }

    public Enrollment? Enrollment { get; private set; }

    /// <summary>Starts a payment at the moment a Stripe Checkout Session is created for it.</summary>
    public static Payment StartCheckout(
        Guid studentId,
        Guid courseId,
        string courseTitle,
        decimal amount,
        string currency,
        string checkoutSessionId) =>
        new()
        {
            StudentId = studentId,
            CourseId = courseId,
            CourseTitle = courseTitle,
            Amount = amount,
            Currency = currency,
            Status = PaymentStatus.Pending,
            ProviderCheckoutSessionId = checkoutSessionId
        };

    /// <summary>
    /// Confirms the payment and records the enrolment it unlocked.
    ///
    /// A no-op past the first call rather than a thrown exception: Stripe redelivers webhooks, and
    /// a retry landing here after the first one already settled the payment is the ordinary case,
    /// not an error.
    /// </summary>
    public void MarkSucceeded(string paymentIntentId, Guid enrollmentId, DateTimeOffset paidAtUtc)
    {
        if (Status != PaymentStatus.Pending)
        {
            return;
        }

        ProviderPaymentIntentId = paymentIntentId;
        EnrollmentId = enrollmentId;
        PaidAtUtc = paidAtUtc;
        Status = PaymentStatus.Succeeded;

        RaiseDomainEvent(new PaymentSucceededDomainEvent(
            Id, StudentId, CourseId, CourseTitle, Amount, Currency));
    }

    /// <summary>Records that the checkout was abandoned or the card was declined.</summary>
    public void MarkFailed(string? reason)
    {
        if (Status != PaymentStatus.Pending)
        {
            return;
        }

        FailureReason = reason;
        Status = PaymentStatus.Failed;
    }

    /// <summary>Records that the checkout session's own time limit passed unpaid.</summary>
    public void MarkExpired()
    {
        if (Status != PaymentStatus.Pending)
        {
            return;
        }

        Status = PaymentStatus.Expired;
    }

    /// <summary>
    /// Records a refund already carried out at the gateway. This is a ledger entry, not the act
    /// of refunding: the caller talks to Stripe first and only calls this once that succeeded.
    /// </summary>
    public void RecordRefund(decimal amount, DateTimeOffset refundedAtUtc)
    {
        if (Status is not (PaymentStatus.Succeeded or PaymentStatus.PartiallyRefunded))
        {
            throw new InvalidOperationException(
                $"Cannot refund a payment in the {Status} state.");
        }

        decimal totalRefunded = (RefundedAmount ?? 0) + amount;

        if (totalRefunded > Amount)
        {
            throw new InvalidOperationException("Cannot refund more than was paid.");
        }

        RefundedAmount = totalRefunded;
        RefundedAtUtc = refundedAtUtc;
        Status = totalRefunded == Amount ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;
    }
}
