using MediatR;
using Microsoft.Extensions.Logging;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Payments;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Payments.ProcessWebhook;

/// <summary>
/// A Stripe webhook delivery, still as raw bytes: the payload and the signature header, exactly
/// as the request carried them. Signature verification needs the untouched body, so nothing
/// upstream of this command is allowed to have parsed or reserialised it.
/// </summary>
public sealed record ProcessStripeWebhookCommand(string Payload, string SignatureHeader) : IRequest<Result>;

/// <summary>
/// Settles a webhook delivery.
///
/// Two independent guards against the same failure mode — Stripe's delivery is at-least-once, so
/// every event can arrive more than once. The processed-events ledger stops a redelivery from
/// running any handling code a second time; failing that, every state change this handler makes
/// is itself a no-op past the first call (<see cref="Payment.MarkSucceeded"/>, and reusing an
/// existing active enrolment rather than creating a second one). Belt and braces, because the
/// failure mode here is a duplicate enrolment or a payment double-booked, not a cosmetic glitch.
/// </summary>
public sealed class ProcessStripeWebhookCommandHandler(
    IPaymentGateway gateway,
    IPaymentRepository payments,
    IEnrollmentRepository enrollments,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ILogger<ProcessStripeWebhookCommandHandler> logger)
    : IRequestHandler<ProcessStripeWebhookCommand, Result>
{
    public async Task<Result> Handle(ProcessStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        GatewayWebhookEvent webhookEvent;
        try
        {
            webhookEvent = gateway.ParseWebhookEvent(request.Payload, request.SignatureHeader);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Rejected a webhook delivery with an invalid signature.");
            return Result.Failure(PaymentErrors.InvalidWebhookSignature);
        }

        if (await payments.IsWebhookEventProcessedAsync(webhookEvent.EventId, cancellationToken))
        {
            return Result.Success();
        }

        switch (webhookEvent.EventType)
        {
            case "checkout.session.completed":
                await HandleCompletedAsync(webhookEvent, cancellationToken);
                break;

            case "checkout.session.expired":
                await HandleExpiredAsync(webhookEvent, cancellationToken);
                break;

            // Any other event type Stripe sends to this endpoint (the account may be subscribed to
            // more than this integration acts on) is acknowledged without side effects, so Stripe
            // stops retrying it.
        }

        await payments.MarkWebhookEventProcessedAsync(
            webhookEvent.EventId, dateTimeProvider.UtcNow, cancellationToken);

        // One SaveChanges for the whole delivery: the enrolment, the payment's new status and the
        // processed-event marker land together, so a crash mid-handler cannot leave a payment
        // settled without its enrolment or an event marked processed without its side effects.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task HandleCompletedAsync(GatewayWebhookEvent webhookEvent, CancellationToken cancellationToken)
    {
        if (webhookEvent.CheckoutSessionId is null)
        {
            return;
        }

        Payment? payment = await payments.GetByCheckoutSessionIdAsync(
            webhookEvent.CheckoutSessionId, cancellationToken);

        if (payment is null)
        {
            logger.LogWarning(
                "A completed checkout arrived for session {SessionId}, which no payment references.",
                webhookEvent.CheckoutSessionId);
            return;
        }

        // Already settled: a redelivery, or the rare case of the same course paid for in two tabs.
        if (payment.Status != PaymentStatus.Pending)
        {
            return;
        }

        // Reuses an active enrolment if the double-tab race above already produced one, rather
        // than risk a second row for the same student and course.
        Enrollment? enrollment = await enrollments.GetActiveAsync(
            payment.StudentId, payment.CourseId, cancellationToken);

        if (enrollment is null)
        {
            enrollment = Enrollment.Create(payment.StudentId, payment.CourseId, dateTimeProvider.UtcNow);
            await enrollments.AddAsync(enrollment, cancellationToken);
        }

        payment.MarkSucceeded(
            webhookEvent.PaymentIntentId ?? string.Empty, enrollment.Id, dateTimeProvider.UtcNow);
    }

    private async Task HandleExpiredAsync(GatewayWebhookEvent webhookEvent, CancellationToken cancellationToken)
    {
        if (webhookEvent.CheckoutSessionId is null)
        {
            return;
        }

        Payment? payment = await payments.GetByCheckoutSessionIdAsync(
            webhookEvent.CheckoutSessionId, cancellationToken);

        payment?.MarkExpired();
    }
}
