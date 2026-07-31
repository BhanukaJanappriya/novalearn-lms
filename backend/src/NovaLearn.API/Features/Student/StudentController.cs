using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using NovaLearn.API.Common;
using NovaLearn.Application.Features.Student.Dashboard;
using NovaLearn.Shared.Results;

namespace NovaLearn.API.Features.Student;

/// <summary>
/// Learner-facing endpoints. Everything here is scoped to the caller's own record, so the
/// blanket <c>[Authorize]</c> is the whole authorisation story: there is no id to tamper with.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/student")]
[Authorize]
public sealed class StudentController(ISender sender) : ApiControllerBase
{
    /// <summary>Returns the caller's dashboard (summary, courses in progress, categories, activity, suggestions).</summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(StudentDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        Result<StudentDashboardResponse> result =
            await sender.Send(new GetStudentDashboardQuery(), cancellationToken);

        return HandleResult(result);
    }
}
