using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLearn.API.Common;
using NovaLearn.Application.Features.Support.Common;
using NovaLearn.Application.Features.Support.CreateTicket;
using NovaLearn.Application.Features.Support.GetMyTickets;
using NovaLearn.Application.Features.Support.GetTicket;
using NovaLearn.Application.Features.Support.ReplyToTicket;
using NovaLearn.Domain.Support;
using NovaLearn.Shared.Results;

namespace NovaLearn.API.Features.Support;

/// <summary>Request body for raising a new ticket.</summary>
public sealed record CreateTicketRequest(
    string Subject, TicketCategory Category, TicketPriority Priority, string Message);

/// <summary>Request body for a reply. IsInternalNote is silently ignored from a non-staff caller — see the handler.</summary>
public sealed record ReplyRequest(string Body, bool IsInternalNote = false);

/// <summary>
/// Support tickets from the submitter's own side: raise one, see your own, reply to your own.
/// Anyone signed in may use this — every role submits tickets the same way, staff included.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/support/tickets")]
[Authorize]
public sealed class SupportController(ISender sender) : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTicket(
        CreateTicketRequest request, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(
            new CreateTicketCommand(request.Subject, request.Category, request.Priority, request.Message),
            cancellationToken));

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TicketSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyTickets(CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetMyTicketsQuery(), cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTicket(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetTicketQuery(id), cancellationToken));

    [HttpPost("{id:guid}/replies")]
    [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reply(
        Guid id, ReplyRequest request, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(
            new ReplyToTicketCommand(id, request.Body, request.IsInternalNote), cancellationToken));
}
