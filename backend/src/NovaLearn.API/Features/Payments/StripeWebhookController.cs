using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLearn.API.Common;
using NovaLearn.Application.Features.Payments.ProcessWebhook;
using NovaLearn.Shared.Results;

namespace NovaLearn.API.Features.Payments;

/// <summary>
/// Receives Stripe's webhook deliveries. Deliberately its own controller, entirely unauthenticated
/// by any of the usual means: Stripe cannot carry a bearer token, so the only gate here is the
/// signature this action verifies before anything it delivers is trusted.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/payments/webhook")]
[AllowAnonymous]
public sealed class StripeWebhookController(ISender sender) : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Receive(CancellationToken cancellationToken)
    {
        // Read manually rather than bind a body model: signature verification needs the exact
        // bytes Stripe sent, and nothing upstream of this line is allowed to have touched them.
        using var reader = new StreamReader(Request.Body);
        string payload = await reader.ReadToEndAsync(cancellationToken);
        string signature = Request.Headers["Stripe-Signature"].ToString();

        Result result = await sender.Send(
            new ProcessStripeWebhookCommand(payload, signature), cancellationToken);

        return HandleResult(result);
    }
}
