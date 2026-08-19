using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Common.Errors;

/// <summary>Central catalogue of payment and checkout failures.</summary>
public static class PaymentErrors
{
    public static readonly Error Unauthenticated =
        Error.Unauthorized("payment.unauthenticated", "You must be signed in to pay for a course.");

    public static readonly Error Forbidden =
        Error.Forbidden("payment.forbidden", "Only an administrator can view or act on the finance ledger.");

    public static readonly Error CourseNotFound =
        Error.NotFound("payment.course_not_found", "The requested course was not found.");

    public static readonly Error CourseNotPublished =
        Error.Conflict("payment.course_not_published", "This course is not open for enrolment yet.");

    public static readonly Error AlreadyEnrolled =
        Error.Conflict("payment.already_enrolled", "You are already enrolled in this course.");

    public static readonly Error CourseIsFree =
        Error.Conflict(
            "payment.course_is_free",
            "This course is free — enrol directly instead of starting a checkout.");

    public static readonly Error NotFound =
        Error.NotFound("payment.not_found", "The requested payment was not found.");

    public static readonly Error NotRefundable =
        Error.Conflict("payment.not_refundable", "Only a paid, unrefunded payment can be refunded.");

    public static readonly Error RefundExceedsRemaining =
        Error.Validation(
            "payment.refund_exceeds_remaining", "The refund amount is more than is left to refund.");

    public static readonly Error RefundAmountMustBePositive =
        Error.Validation("payment.refund_amount_invalid", "The refund amount must be greater than zero.");

    public static Error GatewayError(string detail) =>
        Error.Failure("payment.gateway_error", detail);

    public static readonly Error InvalidWebhookSignature =
        Error.Validation("payment.invalid_webhook_signature", "The webhook signature could not be verified.");

    public static readonly Error ConcurrentModification =
        Error.Conflict(
            "payment.concurrent_modification",
            "This payment changed while the refund was in progress. Reload and try again.");
}
