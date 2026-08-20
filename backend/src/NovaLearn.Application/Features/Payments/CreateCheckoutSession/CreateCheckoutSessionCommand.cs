using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Payments.Common;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Payments;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Payments.CreateCheckoutSession;

/// <summary>Starts a Stripe Checkout for a paid course. Free courses are enrolled directly instead.</summary>
public sealed record CreateCheckoutSessionCommand(Guid CourseId) : IRequest<Result<CheckoutSessionDto>>;

public sealed class CreateCheckoutSessionCommandHandler(
    ICourseRepository courses,
    IEnrollmentRepository enrollments,
    IPaymentRepository payments,
    IPaymentGateway gateway,
    IFrontendUrls frontendUrls,
    ISettingsProvider settings,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCheckoutSessionCommand, Result<CheckoutSessionDto>>
{
    public async Task<Result<CheckoutSessionDto>> Handle(
        CreateCheckoutSessionCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } studentId)
        {
            return Result.Failure<CheckoutSessionDto>(PaymentErrors.Unauthenticated);
        }

        Course? course = await courses.GetByIdAsync(request.CourseId, cancellationToken);
        if (course is null)
        {
            return Result.Failure<CheckoutSessionDto>(PaymentErrors.CourseNotFound);
        }

        if (course.Status != CourseStatus.Published)
        {
            return Result.Failure<CheckoutSessionDto>(PaymentErrors.CourseNotPublished);
        }

        if (course.Price <= 0)
        {
            return Result.Failure<CheckoutSessionDto>(PaymentErrors.CourseIsFree);
        }

        Enrollment? existing = await enrollments.GetActiveAsync(studentId, course.Id, cancellationToken);
        if (existing is not null)
        {
            return Result.Failure<CheckoutSessionDto>(PaymentErrors.AlreadyEnrolled);
        }

        PlatformSettingsSnapshot platform = await settings.GetAsync(cancellationToken);

        // Stripe substitutes {CHECKOUT_SESSION_ID} into the success URL itself; the success page
        // uses it only to show the right confirmation, never to decide that payment happened —
        // that decision is the webhook's alone.
        string successUrl = frontendUrls.Build(
            $"/checkout/success?session_id={{CHECKOUT_SESSION_ID}}&courseId={course.Id}");
        string cancelUrl = frontendUrls.Build($"/checkout/cancelled?courseId={course.Id}");

        CheckoutSessionResult session;
        try
        {
            session = await gateway.CreateCheckoutSessionAsync(
                new CheckoutSessionRequest(
                    course.Title,
                    course.Price,
                    platform.DefaultCurrency,
                    successUrl,
                    cancelUrl,
                    Metadata: new Dictionary<string, string>
                    {
                        ["courseId"] = course.Id.ToString(),
                        ["studentId"] = studentId.ToString()
                    }),
                cancellationToken);
        }
        catch (Exception exception)
        {
            return Result.Failure<CheckoutSessionDto>(PaymentErrors.GatewayError(exception.Message));
        }

        Payment payment = Payment.StartCheckout(
            studentId, course.Id, course.Title, course.Price, platform.DefaultCurrency, session.SessionId);

        await payments.AddAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CheckoutSessionDto(session.CheckoutUrl));
    }
}
