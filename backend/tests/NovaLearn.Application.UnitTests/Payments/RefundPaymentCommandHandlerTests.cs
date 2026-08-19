using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Payments.Common;
using NovaLearn.Application.Features.Payments.RefundPayment;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Payments;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Payments;

public sealed class RefundPaymentCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);

    private readonly IPaymentRepository _payments = Substitute.For<IPaymentRepository>();
    private readonly IPaymentGateway _gateway = Substitute.For<IPaymentGateway>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly RefundPaymentCommandHandler _sut;

    public RefundPaymentCommandHandlerTests()
    {
        _sut = new RefundPaymentCommandHandler(_payments, _gateway, _currentUser, _unitOfWork, _clock);
        _clock.UtcNow.Returns(Now);
        SignedInAs(Roles.Administrator);

        _gateway.RefundAsync(Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new RefundResult("re_test_1", 0));
    }

    private void SignedInAs(params string[] roles) =>
        _currentUser.IsInRole(Arg.Any<string>()).Returns(call => roles.Contains(call.Arg<string>()));

    private static Payment PaidPayment(decimal amount = 100m)
    {
        Payment payment = Payment.StartCheckout(
            Guid.NewGuid(), Guid.NewGuid(), "Intro to Programming", amount, "usd", "cs_test_1");
        payment.MarkSucceeded("pi_test_1", Guid.NewGuid(), Now.AddDays(-1));
        return payment;
    }

    private Task<Result<TransactionDto>> Act(Guid paymentId, decimal? amount = null) =>
        _sut.Handle(new RefundPaymentCommand(paymentId, amount), CancellationToken.None);

    [Fact]
    public async Task A_non_administrator_cannot_refund()
    {
        SignedInAs(Roles.Lecturer);
        Payment payment = PaidPayment();
        _payments.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        Result<TransactionDto> result = await Act(payment.Id);

        result.Error.Should().Be(PaymentErrors.Forbidden);
        await _gateway.DidNotReceive().RefundAsync(
            Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_missing_payment_is_reported()
    {
        Guid id = Guid.NewGuid();
        _payments.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Payment?)null);

        Result<TransactionDto> result = await Act(id);

        result.Error.Should().Be(PaymentErrors.NotFound);
    }

    [Fact]
    public async Task A_payment_that_never_succeeded_cannot_be_refunded()
    {
        Payment payment = Payment.StartCheckout(
            Guid.NewGuid(), Guid.NewGuid(), "Intro to Programming", 100m, "usd", "cs_test_1");
        _payments.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        Result<TransactionDto> result = await Act(payment.Id);

        result.Error.Should().Be(PaymentErrors.NotRefundable);
    }

    [Fact]
    public async Task Requesting_more_than_is_left_is_refused_without_calling_the_gateway()
    {
        Payment payment = PaidPayment(amount: 100m);
        _payments.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        Result<TransactionDto> result = await Act(payment.Id, amount: 150m);

        result.Error.Should().Be(PaymentErrors.RefundExceedsRemaining);
        await _gateway.DidNotReceive().RefundAsync(
            Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task No_amount_refunds_everything_still_left()
    {
        Payment payment = PaidPayment(amount: 80m);
        _payments.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        await Act(payment.Id, amount: null);

        await _gateway.Received(1).RefundAsync("pi_test_1", 80m, "usd", Arg.Any<CancellationToken>());
        payment.RefundedAmount.Should().Be(80m);
        payment.Status.Should().Be(PaymentStatus.Refunded);
    }

    [Fact]
    public async Task A_gateway_failure_leaves_the_ledger_untouched()
    {
        Payment payment = PaidPayment(amount: 80m);
        _payments.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);
        _gateway.RefundAsync(Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<RefundResult>(_ => throw new InvalidOperationException("Stripe is down"));

        Result<TransactionDto> result = await Act(payment.Id);

        result.IsFailure.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Succeeded, "nothing was actually refunded at the gateway");
        payment.RefundedAmount.Should().BeNull();
    }

    [Fact]
    public async Task A_partial_refund_is_reflected_in_the_returned_row()
    {
        Payment payment = PaidPayment(amount: 100m);
        _payments.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        Result<TransactionDto> result = await Act(payment.Id, amount: 25m);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(PaymentStatus.PartiallyRefunded);
        result.Value.RefundedAmount.Should().Be(25m);
        result.Value.CanRefund.Should().BeTrue();
        result.Value.RefundableAmount.Should().Be(75m);
    }
}
