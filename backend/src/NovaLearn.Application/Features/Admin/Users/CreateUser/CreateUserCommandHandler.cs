using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Admin.Users.Common;
using NovaLearn.Domain.Audit;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Admin.Users.CreateUser;

public sealed class CreateUserCommandHandler(
    IUserAdministration users,
    IUserDirectory directory,
    ICurrentUser currentUser,
    IAuditLogger auditLogger)
    : IRequestHandler<CreateUserCommand, Result<AdminUserDto>>
{
    public async Task<Result<AdminUserDto>> Handle(
        CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId)
        {
            return Result.Failure<AdminUserDto>(UserAdminErrors.Unauthenticated);
        }

        // Same privilege-escalation guard as editing roles: only a super administrator may mint
        // a peer at their own level.
        if (request.Role == Roles.SuperAdministrator && !UserAdminPolicy.IsSuperAdmin(currentUser))
        {
            return Result.Failure<AdminUserDto>(UserAdminErrors.CannotGrantSuperAdmin);
        }

        Result<Guid> created = await users.CreateAccountAsync(
            request.Email.Trim(),
            request.FirstName.Trim(),
            request.LastName.Trim(),
            request.Role,
            request.Password,
            cancellationToken);

        if (created.IsFailure)
        {
            return Result.Failure<AdminUserDto>(created.Error);
        }

        AdminUserRow? row = await directory.GetAsync(created.Value, cancellationToken);
        if (row is null)
        {
            return Result.Failure<AdminUserDto>(UserAdminErrors.NotFound);
        }

        await auditLogger.RecordAsync(
            callerId,
            AuditCategory.UserManagement,
            "Created account",
            $"Added {row.FullName} ({row.Email}) as {request.Role}",
            "User",
            row.Id,
            cancellationToken);

        return AdminUserDto.FromRow(row);
    }
}
