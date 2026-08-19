using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLearn.API.Common;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Payments.Common;
using NovaLearn.Application.Features.Payments.GetFinanceOverview;
using NovaLearn.Application.Features.Payments.GetTransactions;
using NovaLearn.Application.Features.Payments.RefundPayment;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Payments;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.API.Features.Admin;

/// <summary>Body for issuing a refund. A null amount refunds everything still left on the payment.</summary>
public sealed record RefundRequest(decimal? Amount);

/// <summary>
/// The finance ledger: revenue, trend, course breakdown, and every transaction, with refunds.
/// Administrator only — this is the one place in the admin console that moves real money, so it
/// carries no lecturer-scoped view the way assessments and the resource wall do.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/finance")]
[Authorize(Roles = $"{Roles.SuperAdministrator},{Roles.Administrator}")]
public sealed class FinanceController(ISender sender) : ApiControllerBase
{
    /// <summary>Headline figures, revenue trend and course breakdown for a window.</summary>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(FinanceOverview), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview(
        [FromQuery] int days = 30, CancellationToken cancellationToken = default) =>
        HandleResult(await sender.Send(new GetFinanceOverviewQuery(days), cancellationToken));

    /// <summary>The ledger: every payment, newest first, with optional filters.</summary>
    [HttpGet("transactions")]
    [ProducesResponseType(typeof(PagedResult<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] PaymentStatus? status,
        [FromQuery] string? search,
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        HandleResult(await sender.Send(
            new GetTransactionsQuery(status, search, fromUtc, toUtc, page, pageSize), cancellationToken));

    /// <summary>Refunds a payment, in whole or in part.</summary>
    [HttpPost("transactions/{id:guid}/refund")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Refund(
        Guid id, RefundRequest request, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new RefundPaymentCommand(id, request.Amount), cancellationToken));
}
