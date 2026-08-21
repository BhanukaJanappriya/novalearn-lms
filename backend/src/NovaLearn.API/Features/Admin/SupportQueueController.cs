using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLearn.API.Common;
using NovaLearn.API.Features.Support;
using NovaLearn.Application.Features.Support.AssignTicket;
using NovaLearn.Application.Features.Support.ChangeTicketPriority;
using NovaLearn.Application.Features.Support.ChangeTicketStatus;
using NovaLearn.Application.Features.Support.Common;
using NovaLearn.Application.Features.Support.GetStaffTicketCounts;
using NovaLearn.Application.Features.Support.GetStaffTickets;
using NovaLearn.Application.Features.Support.GetTicket;
using NovaLearn.Application.Features.Support.ReplyToTicket;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Support;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.API.Features.Admin;

/// <summary>Request body for reassigning a ticket. A null id returns it to the unclaimed queue.</summary>
public sealed record AssignTicketRequest(Guid? AssignedToId);

/// <summary>Request body for moving a ticket to a new status.</summary>
public sealed record ChangeStatusRequest(TicketStatus Status);

/// <summary>Request body for re-prioritising a ticket.</summary>
public sealed record ChangePriorityRequest(TicketPriority Priority);

/// <summary>
/// The staff side of support: the full queue, triage, and replying as staff. Administrator only —
/// this mirrors finance and settings rather than the assessment or resource slices, which give a
/// lecturer their own scoped view. Support has no such scope; an administrator sees every ticket.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/support")]
[Authorize(Roles = $"{Roles.SuperAdministrator},{Roles.Administrator}")]
public sealed class SupportQueueController(ISender sender) : ApiControllerBase
{
    [HttpGet("tickets")]
    [ProducesResponseType(typeof(PagedResult<TicketSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTickets(
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketCategory? category,
        [FromQuery] TicketPriority? priority,
        [FromQuery] Guid? assignedToId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        HandleResult(await sender.Send(
            new GetStaffTicketsQuery(status, category, priority, assignedToId, search, page, pageSize),
            cancellationToken));

    [HttpGet("tickets/counts")]
    [ProducesResponseType(typeof(StaffTicketCounts), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCounts(CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetStaffTicketCountsQuery(), cancellationToken));

    [HttpGet("tickets/{id:guid}")]
    [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTicket(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetTicketQuery(id), cancellationToken));

    /// <summary>Replies as staff. Set IsInternalNote to leave a note only staff can see.</summary>
    [HttpPost("tickets/{id:guid}/replies")]
    [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reply(
        Guid id, ReplyRequest request, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(
            new ReplyToTicketCommand(id, request.Body, request.IsInternalNote), cancellationToken));

    [HttpPut("tickets/{id:guid}/status")]
    [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStatus(
        Guid id, ChangeStatusRequest request, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new ChangeTicketStatusCommand(id, request.Status), cancellationToken));

    [HttpPut("tickets/{id:guid}/assignment")]
    [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(
        Guid id, AssignTicketRequest request, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new AssignTicketCommand(id, request.AssignedToId), cancellationToken));

    [HttpPut("tickets/{id:guid}/priority")]
    [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePriority(
        Guid id, ChangePriorityRequest request, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new ChangeTicketPriorityCommand(id, request.Priority), cancellationToken));
}
