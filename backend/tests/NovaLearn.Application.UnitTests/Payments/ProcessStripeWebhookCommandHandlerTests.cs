using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Payments.ProcessWebhook;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Payments;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Payments;

public sealed class ProcessStripeWebhookCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);

    private readonly IPaymentGateway _gateway = Substitute.For<IPaymentGateway>();
    private readonly IPaymentRepository _payments = Substitute.For<IPaymentRepository>();
    private readonly IEnrollmentRepository _enrollments = Substitute.For<IEnrollmentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly Guid _studentId = Guid.NewGuid();
    private readonly Guid _courseId = Guid.NewGuid();
    private readonly ProcessStripeWebhookCommandHandler _sut;

    public ProcessStripeWebhookCommandHandlerTests()
    {
        _sut = new ProcessStripeWebhookCommandHandler(
            _gateway, _payments, _enrollments, _unitOfWork, _clock,
            Substitute.For<ILogger<ProcessStripeWebhookCommandHandler>>());

        _clock.UtcNow.Returns(Now);
        _payments.IsWebhookEventProcessedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _enrollments.GetActiveAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Enrollment?)null);
    }

    private Payment PendingPayment(decimal amount = 50m) =>
        Payment.StartCheckout(_studentId, _courseId, "Intro to Programming", amount, "usd", "cs_test_1");

    private Task<Result> Act(string eventType, string? sessionId = "cs_test_1", string? paymentIntentId = "pi_1")
    {
        _gateway.ParseWebhookEvent("payload", "sig")
            .Returns(new GatewayWebhookEvent("evt_1", eventType, sessionId, paymentIntentId));

        return _sut.Handle(new ProcessStripeWebhookCommand("payload", "sig"), CancellationToken.None);
    }

    [Fact]
    public async Task An_invalid_signature_is_rejected_before_anything_is_looked_up()
    {
        _gateway.ParseWebhookEvent("payload", "sig")
            .Returns<GatewayWebhookEvent>(_ => throw new InvalidOperationException("bad signature"));

        Result result = await _sut.Handle(
            new ProcessStripeWebhookCommand("payload", "sig"), CancellationToken.None);

        result.Error.Should().Be(PaymentErrors.InvalidWebhookSignature);
        await _payments.DidNotReceive().GetByCheckoutSessionIdAsync(
            Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_already_processed_event_is_a_pure_no_op()
    {
        _payments.IsWebhookEventProcessedAsync("evt_1", Arg.Any<CancellationToken>()).Returns(true);

        Result result = await Act("checkout.session.completed");

        result.IsSuccess.Should().BeTrue();
        await _payments.DidNotReceive().GetByCheckoutSessionIdAsync(
            Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_completed_checkout_creates_the_enrolment_and_settles_the_payment()
    {
        Payment payment = PendingPayment();
        _payments.GetByCheckoutSessionIdAsync("cs_test_1", Arg.Any<CancellationToken>()).Returns(payment);

        Result result = await Act("checkout.session.completed");

        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.ProviderPaymentIntentId.Should().Be("pi_1");

        await _enrollments.Received(1).AddAsync(
            Arg.Is<Enrollment>(e => e.StudentId == _studentId && e.CourseId == _courseId),
            Arg.Any<CancellationToken>());

        await _payments.Received(1).MarkWebhookEventProcessedAsync(
            "evt_1", Now, Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_redelivered_completed_event_does_not_create_a_second_enrolment()
    {
        Payment payment = PendingPayment();
        payment.MarkSucceeded("pi_1", Guid.NewGuid(), Now.AddMinutes(-5));
        _payments.GetByCheckoutSessionIdAsync("cs_test_1", Arg.Any<CancellationToken>()).Returns(payment);

        Result result = await Act("checkout.session.completed");

        result.IsSuccess.Should().BeTrue();
        await _enrollments.DidNotReceive().AddAsync(Arg.Any<Enrollment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_double_tab_race_reuses_the_enrolment_the_other_tab_already_created()
    {
        Payment payment = PendingPayment();
        _payments.GetByCheckoutSessionIdAsync("cs_test_1", Arg.Any<CancellationToken>()).Returns(payment);

        Enrollment alreadyThere = Enrollment.Create(_studentId, _courseId, Now.AddMinutes(-1));
        _enrollments.GetActiveAsync(_studentId, _courseId, Arg.Any<CancellationToken>())
            .Returns(alreadyThere);

        await Act("checkout.session.completed");

        payment.EnrollmentId.Should().Be(alreadyThere.Id);
        await _enrollments.DidNotReceive().AddAsync(Arg.Any<Enrollment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_completed_event_for_an_unknown_session_is_ignored_rather_than_thrown()
    {
        _payments.GetByCheckoutSessionIdAsync("cs_test_1", Arg.Any<CancellationToken>())
            .Returns((Payment?)null);

        Result result = await Act("checkout.session.completed");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task An_expired_checkout_marks_the_payment_expired()
    {
        Payment payment = PendingPayment();
        _payments.GetByCheckoutSessionIdAsync("cs_test_1", Arg.Any<CancellationToken>()).Returns(payment);

        await Act("checkout.session.expired");

        payment.Status.Should().Be(PaymentStatus.Expired);
    }

    [Fact]
    public async Task An_event_type_this_integration_does_not_act_on_is_still_acknowledged()
    {
        Result result = await Act("payment_intent.created");

        result.IsSuccess.Should().BeTrue();
        await _payments.Received(1).MarkWebhookEventProcessedAsync(
            "evt_1", Now, Arg.Any<CancellationToken>());
    }
}
