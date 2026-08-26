using FluentValidation;
using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Admin.Departments.Common;
using NovaLearn.Domain.Audit;
using NovaLearn.Domain.Departments;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Admin.Departments.SaveDepartment;

/// <summary>
/// Creates or updates a department. One command for both, because the fields and every rule
/// about them are identical either way.
/// </summary>
public sealed record SaveDepartmentCommand(
    /// <summary>Null creates a department; an id updates that one.</summary>
    Guid? DepartmentId,
    string Name,
    string Code,
    string? Description,
    Guid? HeadId,
    bool IsActive) : IRequest<Result<DepartmentDto>>;

public sealed class SaveDepartmentCommandValidator : AbstractValidator<SaveDepartmentCommand>
{
    public SaveDepartmentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);

        // Codes are shown in tight table cells and used as a human handle, so keep them short.
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(20)
            .Matches("^[A-Za-z0-9-]+$")
            .WithMessage("A code may only contain letters, numbers and hyphens.");

        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public sealed class SaveDepartmentCommandHandler(
    IDepartmentRepository departments,
    IUserDirectory users,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLogger auditLogger)
    : IRequestHandler<SaveDepartmentCommand, Result<DepartmentDto>>
{
    public async Task<Result<DepartmentDto>> Handle(
        SaveDepartmentCommand request, CancellationToken cancellationToken)
    {
        if (await departments.CodeExistsAsync(request.Code, request.DepartmentId, cancellationToken))
        {
            return Result.Failure<DepartmentDto>(DepartmentErrors.CodeInUse);
        }

        if (await CheckHeadAsync(request.HeadId, cancellationToken) is { } headError)
        {
            return Result.Failure<DepartmentDto>(headError);
        }

        Department department;
        bool isNew = request.DepartmentId is null;

        if (request.DepartmentId is { } id)
        {
            Department? existing = await departments.GetByIdAsync(id, cancellationToken);
            if (existing is null)
            {
                return Result.Failure<DepartmentDto>(DepartmentErrors.NotFound);
            }

            existing.Update(request.Name, request.Code, request.Description, request.HeadId, request.IsActive);
            department = existing;
        }
        else
        {
            department = Department.Create(
                request.Name, request.Code, request.Description, request.HeadId, request.IsActive);

            await departments.AddAsync(department, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogger.RecordAsync(
            currentUser.UserId!.Value,
            AuditCategory.Departments,
            isNew ? "Created department" : "Updated department",
            department.Name,
            "Department",
            department.Id,
            cancellationToken);

        // Re-read so the head's name comes back populated on a freshly assigned head.
        Department? saved = await departments.GetByIdAsync(department.Id, cancellationToken);

        return saved is null
            ? Result.Failure<DepartmentDto>(DepartmentErrors.NotFound)
            : DepartmentDto.FromEntity(saved, 0);
    }

    /// <summary>A head has to be someone who actually teaches or administers.</summary>
    private async Task<Error?> CheckHeadAsync(Guid? headId, CancellationToken cancellationToken)
    {
        if (headId is not { } id)
        {
            return null;
        }

        var head = await users.GetAsync(id, cancellationToken);
        if (head is null)
        {
            return DepartmentErrors.HeadNotALecturer;
        }

        bool eligible =
            head.Roles.Contains(Roles.Lecturer)
            || head.Roles.Contains(Roles.Administrator)
            || head.Roles.Contains(Roles.SuperAdministrator);

        return eligible ? null : DepartmentErrors.HeadNotALecturer;
    }
}
