using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Security.Common;
using NovaLearn.Domain.Audit;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Security.RevokeAllSessionsForUser;

/// <summary>Signs an account out everywhere by revoking every currently active session it holds. Staff only.</summary>
public sealed record RevokeAllSessionsForUserCommand(Guid UserId) : IRequest<Result>;

public sealed class RevokeAllSessionsForUserCommandHandler(
    IRefreshTokenRepository refreshTokens,
    IUserDirectory directory,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    IAuditLogger auditLogger)
    : IRequestHandler<RevokeAllSessionsForUserCommand, Result>
{
    public async Task<Result> Handle(RevokeAllSessionsForUserCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId || !SecurityAuthority.IsStaff(currentUser))
        {
            return Result.Failure(SecurityErrors.StaffOnly);
        }

        var target = await directory.GetAsync(request.UserId, cancellationToken);
        if (target is null)
        {
            return Result.Failure(UserAdminErrors.NotFound);
        }

        DateTimeOffset now = dateTimeProvider.UtcNow;

        await refreshTokens.RevokeAllActiveForUserAsync(request.UserId, now, currentUser.IpAddress, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogger.RecordAsync(
            callerId,
            AuditCategory.Security,
            "Revoked all sessions",
            target.FullName,
            "User",
            target.Id,
            cancellationToken);

        return Result.Success();
    }
}
