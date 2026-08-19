using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLearn.API.Common;
using NovaLearn.Application.Features.Payments.Common;
using NovaLearn.Application.Features.Payments.CreateCheckoutSession;
using NovaLearn.Shared.Results;

namespace NovaLearn.API.Features.Payments;

/// <summary>Request body for starting a checkout.</summary>
public sealed record CreateCheckoutSessionRequest(Guid CourseId);

/// <summary>
/// Starts a Stripe Checkout for a paid course. The webhook that settles it lives in
/// <see cref="StripeWebhookController"/>, a separate, unauthenticated controller — mixing an
/// unauthenticated action into an otherwise-authorized class is exactly the kind of thing a later
/// edit accidentally locks down or accidentally opens up.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/payments")]
[Authorize]
public sealed class PaymentsController(ISender sender) : ApiControllerBase
{
    /// <summary>Starts checkout for a course. The caller is redirected to the returned URL to pay.</summary>
    [HttpPost("checkout-sessions")]
    [ProducesResponseType(typeof(CheckoutSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCheckoutSession(
        CreateCheckoutSessionRequest request, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(
            new CreateCheckoutSessionCommand(request.CourseId), cancellationToken));
}
