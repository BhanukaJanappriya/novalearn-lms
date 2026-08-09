using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using NovaLearn.API.Common;
using NovaLearn.Application.Features.Admin.Departments.Common;
using NovaLearn.Application.Features.Admin.Departments.DeleteDepartment;
using NovaLearn.Application.Features.Admin.Departments.GetDepartments;
using NovaLearn.Application.Features.Admin.Departments.SaveDepartment;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.API.Features.Admin;

/// <summary>
/// Academic departments.
///
/// Reading is open to lecturers as well as admins, because the course form offers a department
/// picker. Changing the institution's structure is an administrator's job, so every mutation is
/// restricted further.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/departments")]
[Authorize(Roles = $"{Roles.Lecturer},{Roles.Administrator},{Roles.SuperAdministrator}")]
public sealed class DepartmentsController(ISender sender) : ApiControllerBase
{
    private const string AdminRoles = $"{Roles.SuperAdministrator},{Roles.Administrator}";

    /// <summary>Lists departments alphabetically, with their head and course count.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetDepartmentsQuery(), cancellationToken));

    /// <summary>Creates a department.</summary>
    [HttpPost]
    [Authorize(Roles = AdminRoles)]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(SaveDepartmentRequest request, CancellationToken cancellationToken)
    {
        var command = new SaveDepartmentCommand(
            null, request.Name, request.Code, request.Description, request.HeadId, request.IsActive);

        Result<DepartmentDto> result = await sender.Send(command, cancellationToken);

        return HandleResult(
            result, department => CreatedAtAction(nameof(List), new { id = department.Id }, department));
    }

    /// <summary>Updates a department.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id, SaveDepartmentRequest request, CancellationToken cancellationToken)
    {
        var command = new SaveDepartmentCommand(
            id, request.Name, request.Code, request.Description, request.HeadId, request.IsActive);

        return HandleResult(await sender.Send(command, cancellationToken));
    }

    /// <summary>Deletes a department. Refused while it still has courses.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new DeleteDepartmentCommand(id), cancellationToken));
}

/// <summary>Body for creating or updating a department.</summary>
public sealed record SaveDepartmentRequest(
    string Name,
    string Code,
    string? Description,
    Guid? HeadId,
    bool IsActive = true);
