using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.AuditLogs.Common;
using NovaLearn.Domain.Audit;
using NovaLearn.Shared.Common;

namespace NovaLearn.Persistence.Repositories;

internal sealed class AuditLogRepository(ApplicationDbContext context) : IAuditLogRepository
{
    public async Task<PagedResult<AuditLogRow>> SearchAsync(
        AuditCategory? category,
        Guid? actorId,
        string? search,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<AuditLog> query = context.Set<AuditLog>()
            .AsNoTracking()
            .Include(a => a.Actor);

        if (category is { } wantedCategory)
        {
            query = query.Where(a => a.Category == wantedCategory);
        }

        if (actorId is { } wantedActor)
        {
            query = query.Where(a => a.ActorId == wantedActor);
        }

        if (fromUtc is { } from)
        {
            query = query.Where(a => a.CreatedAtUtc >= from);
        }

        if (toUtc is { } to)
        {
            query = query.Where(a => a.CreatedAtUtc < to);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            string needle = $"%{search.Trim()}%";

            query = query.Where(a =>
                EF.Functions.ILike(a.Action, needle)
                || (a.Details != null && EF.Functions.ILike(a.Details, needle))
                || (a.Actor != null && EF.Functions.ILike(a.Actor.Email!, needle))
                || (a.Actor != null && EF.Functions.ILike(a.Actor.FirstName + " " + a.Actor.LastName, needle)));
        }

        query = query.OrderByDescending(a => a.CreatedAtUtc);

        int totalCount = await query.CountAsync(cancellationToken);

        List<AuditLog> pageItems = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogRow>(
            pageItems.Select(AuditLogMapper.ToRow).ToList(), page, pageSize, totalCount);
    }
}
