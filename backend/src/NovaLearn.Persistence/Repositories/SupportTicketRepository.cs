using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Support;
using NovaLearn.Shared.Common;

namespace NovaLearn.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the support ticket port.
///
/// "Last activity" — what every list is sorted by — is a computed property on the aggregate that
/// needs the thread loaded, so it cannot come from a plain column. It is still resolved in SQL, as
/// a correlated MAX inside the ORDER BY, so paging narrows down to one page before anything is
/// pulled into memory; a ticket always has at least one message by construction, so the MAX is
/// never over an empty set. <c>AsSplitQuery</c> keeps that ordering query's row count from being
/// multiplied out by the thread it then eager-loads for the page it settled on.
/// </summary>
internal sealed class SupportTicketRepository(ApplicationDbContext context) : ISupportTicketRepository
{
    public async Task AddAsync(SupportTicket ticket, CancellationToken cancellationToken) =>
        await context.SupportTickets.AddAsync(ticket, cancellationToken);

    public async Task AddMessageAsync(SupportTicketMessage message, CancellationToken cancellationToken) =>
        await context.Set<SupportTicketMessage>().AddAsync(message, cancellationToken);

    public Task<SupportTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.SupportTickets
            .Include(t => t.SubmittedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Messages)
                .ThenInclude(m => m.Author)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<SupportTicket>> ListForUserAsync(
        Guid userId, CancellationToken cancellationToken) =>
        await context.SupportTickets
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.SubmittedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Messages)
            .Where(t => t.SubmittedById == userId)
            .OrderByDescending(t => t.Messages.Max(m => m.CreatedAtUtc))
            .ToListAsync(cancellationToken);

    public async Task<PagedResult<SupportTicket>> ListForStaffAsync(
        TicketStatus? status,
        TicketCategory? category,
        TicketPriority? priority,
        Guid? assignedToId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<SupportTicket> query = context.SupportTickets.AsNoTracking();

        if (status is { } wantedStatus)
        {
            query = query.Where(t => t.Status == wantedStatus);
        }

        if (category is { } wantedCategory)
        {
            query = query.Where(t => t.Category == wantedCategory);
        }

        if (priority is { } wantedPriority)
        {
            query = query.Where(t => t.Priority == wantedPriority);
        }

        if (assignedToId is { } wantedAssignee)
        {
            query = query.Where(t => t.AssignedToId == wantedAssignee);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            string needle = $"%{search.Trim()}%";

            query = query.Where(t =>
                EF.Functions.ILike(t.Subject, needle)
                || (t.SubmittedBy != null && EF.Functions.ILike(t.SubmittedBy.Email!, needle))
                || (t.SubmittedBy != null
                    && EF.Functions.ILike(t.SubmittedBy.FirstName + " " + t.SubmittedBy.LastName, needle)));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        // Sorted and paged in SQL, before the thread is ever loaded: only the tickets that
        // survive Skip/Take go on to have their messages fetched at all.
        List<SupportTicket> pageItems = await query
            .OrderByDescending(t => t.Messages.Max(m => m.CreatedAtUtc))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsSplitQuery()
            .Include(t => t.SubmittedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Messages)
            .ToListAsync(cancellationToken);

        return new PagedResult<SupportTicket>(pageItems, page, pageSize, totalCount);
    }

    public async Task<(int Open, int Unassigned, int UrgentOrHigh)> GetStaffCountsAsync(
        CancellationToken cancellationToken)
    {
        int open = await context.SupportTickets
            .AsNoTracking()
            .CountAsync(t => t.Status == TicketStatus.Open, cancellationToken);

        int unassigned = await context.SupportTickets
            .AsNoTracking()
            .CountAsync(
                t => t.AssignedToId == null && t.Status != TicketStatus.Closed, cancellationToken);

        int urgentOrHigh = await context.SupportTickets
            .AsNoTracking()
            .CountAsync(
                t => t.Priority >= TicketPriority.High && t.Status != TicketStatus.Closed,
                cancellationToken);

        return (open, unassigned, urgentOrHigh);
    }
}
