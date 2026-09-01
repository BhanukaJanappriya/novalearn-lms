using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Security.Common;
using NovaLearn.Domain.Audit;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Security.RevokeSession;

/// <summary>Forces a single session to sign out by revoking its refresh token. Staff only.</summary>
public sealed record RevokeSessionCommand(Guid SessionId) : IRequest<Result>;

public sealed class RevokeSessionCommandHandler(
    IRefreshTokenRepository refreshTokens,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    IAuditLogger auditLogger)
    : IRequestHandler<RevokeSessionCommand, Result>
{
    public async Task<Result> Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId || !SecurityAuthority.IsStaff(currentUser))
        {
            return Result.Failure(SecurityErrors.StaffOnly);
        }

        RefreshToken? session = await refreshTokens.GetByIdAsync(request.SessionId, cancellationToken);
        if (session is null)
        {
            return Result.Failure(SecurityErrors.SessionNotFound);
        }

        DateTimeOffset now = dateTimeProvider.UtcNow;

        if (!session.IsActive(now))
        {
            return Result.Failure(SecurityErrors.SessionNotActive);
        }

        session.Revoke(now, currentUser.IpAddress);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogger.RecordAsync(
            callerId,
            AuditCategory.Security,
            "Revoked session",
            session.User?.Email ?? "Unknown",
            "User",
            session.UserId,
            cancellationToken);

        return Result.Success();
    }
}
