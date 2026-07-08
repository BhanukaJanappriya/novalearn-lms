using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Persistence.Repositories;

public sealed class RefreshTokenRepository(ApplicationDbContext dbContext) : IRefreshTokenRepository
{
    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken) =>
        await dbContext.RefreshTokens.AddAsync(token, cancellationToken);

    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

    public async Task RevokeAllActiveForUserAsync(
        Guid userId, DateTimeOffset revokedAtUtc, string? revokedByIp, CancellationToken cancellationToken)
    {
        List<RefreshToken> activeTokens = await dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId
                         && rt.RevokedAtUtc == null
                         && rt.ExpiresAtUtc > revokedAtUtc)
            .ToListAsync(cancellationToken);

        foreach (RefreshToken token in activeTokens)
        {
            token.Revoke(revokedAtUtc, revokedByIp);
        }
    }
}
