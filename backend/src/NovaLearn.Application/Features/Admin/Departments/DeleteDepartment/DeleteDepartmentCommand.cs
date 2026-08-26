using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Audit;
using NovaLearn.Domain.Departments;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Admin.Departments.DeleteDepartment;

public sealed record DeleteDepartmentCommand(Guid DepartmentId) : IRequest<Result>;

public sealed class DeleteDepartmentCommandHandler(
    IDepartmentRepository departments,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLogger auditLogger)
    : IRequestHandler<DeleteDepartmentCommand, Result>
{
    public async Task<Result> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        Department? department = await departments.GetByIdAsync(request.DepartmentId, cancellationToken);
        if (department is null)
        {
            return Result.Failure(DepartmentErrors.NotFound);
        }

        // Deleting out from under live courses would quietly strip their department. Retiring is
        // the right move there, and the error says so.
        IReadOnlyDictionary<Guid, int> counts = await departments.CountCoursesAsync(cancellationToken);
        if (counts.TryGetValue(department.Id, out int courseCount) && courseCount > 0)
        {
            return Result.Failure(DepartmentErrors.HasCourses);
        }

        departments.Remove(department);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogger.RecordAsync(
            currentUser.UserId!.Value,
            AuditCategory.Departments,
            "Deleted department",
            department.Name,
            "Department",
            department.Id,
            cancellationToken);

        return Result.Success();
    }
}
