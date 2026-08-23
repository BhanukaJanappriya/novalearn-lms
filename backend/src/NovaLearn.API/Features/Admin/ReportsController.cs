using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLearn.API.Common;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Payments.Common;
using NovaLearn.Application.Features.Reports.Common;
using NovaLearn.Application.Features.Reports.GetCoursePerformanceReport;
using NovaLearn.Application.Features.Reports.GetEnrollmentsReport;
using NovaLearn.Application.Features.Reports.GetRecentReportRuns;
using NovaLearn.Application.Features.Reports.GetRevenueReport;
using NovaLearn.Application.Features.Reports.GetSupportTicketsReport;
using NovaLearn.Application.Features.Reports.GetUsersReport;
using NovaLearn.Application.Features.Support.Common;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Payments;
using NovaLearn.Domain.Support;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.API.Features.Admin;

/// <summary>
/// The reporting centre: one full, exportable read model per operational domain, each logged as a
/// <c>ReportRun</c> the moment it is generated. Administrator only — every report here spans the
/// whole platform, the same scope as finance and support rather than a lecturer's own courses.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/reports")]
[Authorize(Roles = $"{Roles.SuperAdministrator},{Roles.Administrator}")]
public sealed class ReportsController(ISender sender) : ApiControllerBase
{
    [HttpGet("enrollments")]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentReportRow>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnrollments(
        [FromQuery] EnrollmentStatus? status,
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetEnrollmentsReportQuery(status, fromUtc, toUtc), cancellationToken));

    [HttpGet("revenue")]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenue(
        [FromQuery] PaymentStatus? status,
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetRevenueReportQuery(status, fromUtc, toUtc), cancellationToken));

    [HttpGet("course-performance")]
    [ProducesResponseType(typeof(IReadOnlyList<CoursePerformanceRow>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCoursePerformance(CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetCoursePerformanceReportQuery(), cancellationToken));

    [HttpGet("users")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminUserRow>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] bool? isActive,
        [FromQuery] bool? emailConfirmed,
        CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(
            new GetUsersReportQuery(search, role, isActive, emailConfirmed), cancellationToken));

    [HttpGet("support-tickets")]
    [ProducesResponseType(typeof(IReadOnlyList<TicketSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupportTickets(
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketCategory? category,
        [FromQuery] TicketPriority? priority,
        CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(
            new GetSupportTicketsReportQuery(status, category, priority), cancellationToken));

    /// <summary>The audit panel: who has generated which reports, most recent first.</summary>
    [HttpGet("recent-runs")]
    [ProducesResponseType(typeof(IReadOnlyList<ReportRunDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentRuns(
        [FromQuery] int count = 20, CancellationToken cancellationToken = default) =>
        HandleResult(await sender.Send(new GetRecentReportRunsQuery(count), cancellationToken));
}
