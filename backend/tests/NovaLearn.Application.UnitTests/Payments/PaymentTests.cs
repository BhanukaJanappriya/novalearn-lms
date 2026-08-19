using FluentAssertions;
using NovaLearn.Domain.Payments;
using NovaLearn.Domain.Payments.Events;
using Xunit;

namespace NovaLearn.Application.UnitTests.Payments;

public sealed class PaymentTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);
    private readonly Guid _studentId = Guid.NewGuid();
    private readonly Guid _courseId = Guid.NewGuid();

    private Payment StartedCheckout(decimal amount = 50m) =>
        Payment.StartCheckout(_studentId, _courseId, "Intro to Programming", amount, "usd", "cs_test_123");

    [Fact]
    public void Starting_checkout_leaves_the_payment_pending_with_nothing_paid_yet()
    {
        Payment payment = StartedCheckout();

        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.PaidAtUtc.Should().BeNull();
        payment.EnrollmentId.Should().BeNull();
        payment.ProviderCheckoutSessionId.Should().Be("cs_test_123");
    }

    [Fact]
    public void Marking_succeeded_records_the_enrolment_and_raises_the_domain_event()
    {
        Payment payment = StartedCheckout(amount: 75m);
        Guid enrollmentId = Guid.NewGuid();

        payment.MarkSucceeded("pi_test_456", enrollmentId, Now);

        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.EnrollmentId.Should().Be(enrollmentId);
        payment.ProviderPaymentIntentId.Should().Be("pi_test_456");
        payment.PaidAtUtc.Should().Be(Now);

        payment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PaymentSucceededDomainEvent>()
            .Which.Amount.Should().Be(75m);
    }

    [Fact]
    public void A_redelivered_success_after_the_first_is_a_no_op()
    {
        // Stripe's webhook delivery is at-least-once, so this is the ordinary case, not an error.
        Payment payment = StartedCheckout();
        Guid firstEnrollmentId = Guid.NewGuid();
        payment.MarkSucceeded("pi_first", firstEnrollmentId, Now);
        payment.ClearDomainEvents();

        payment.MarkSucceeded("pi_second", Guid.NewGuid(), Now.AddMinutes(1));

        payment.EnrollmentId.Should().Be(firstEnrollmentId);
        payment.ProviderPaymentIntentId.Should().Be("pi_first");
        payment.DomainEvents.Should().BeEmpty("a settled payment must not notify the learner twice");
    }

    [Fact]
    public void Marking_failed_only_applies_from_pending()
    {
        Payment payment = StartedCheckout();
        payment.MarkSucceeded("pi_1", Guid.NewGuid(), Now);

        payment.MarkFailed("card_declined");

        payment.Status.Should().Be(PaymentStatus.Succeeded, "a settled payment cannot retroactively fail");
    }

    [Fact]
    public void Marking_expired_only_applies_from_pending()
    {
        Payment payment = StartedCheckout();
        payment.MarkSucceeded("pi_1", Guid.NewGuid(), Now);

        payment.MarkExpired();

        payment.Status.Should().Be(PaymentStatus.Succeeded);
    }

    [Fact]
    public void A_full_refund_moves_status_to_refunded()
    {
        Payment payment = StartedCheckout(amount: 100m);
        payment.MarkSucceeded("pi_1", Guid.NewGuid(), Now);

        payment.RecordRefund(100m, Now.AddDays(1));

        payment.Status.Should().Be(PaymentStatus.Refunded);
        payment.RefundedAmount.Should().Be(100m);
    }

    [Fact]
    public void A_partial_refund_moves_status_to_partially_refunded_and_can_be_topped_up()
    {
        Payment payment = StartedCheckout(amount: 100m);
        payment.MarkSucceeded("pi_1", Guid.NewGuid(), Now);

        payment.RecordRefund(30m, Now.AddDays(1));
        payment.Status.Should().Be(PaymentStatus.PartiallyRefunded);
        payment.RefundedAmount.Should().Be(30m);

        payment.RecordRefund(70m, Now.AddDays(2));
        payment.Status.Should().Be(PaymentStatus.Refunded);
        payment.RefundedAmount.Should().Be(100m);
    }

    [Fact]
    public void Refunding_more_than_was_paid_is_refused()
    {
        Payment payment = StartedCheckout(amount: 100m);
        payment.MarkSucceeded("pi_1", Guid.NewGuid(), Now);
        payment.RecordRefund(60m, Now.AddDays(1));

        Action act = () => payment.RecordRefund(60m, Now.AddDays(2));

        act.Should().Throw<InvalidOperationException>();
        payment.RefundedAmount.Should().Be(60m, "the rejected attempt must not partially apply");
    }

    [Fact]
    public void A_payment_that_never_succeeded_cannot_be_refunded()
    {
        Payment payment = StartedCheckout();

        Action act = () => payment.RecordRefund(10m, Now);

        act.Should().Throw<InvalidOperationException>();
    }
}
