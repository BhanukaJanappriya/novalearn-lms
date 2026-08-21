using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Support.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Support.GetStaffTicketCounts;

/// <summary>The queue's at-a-glance header. Staff only.</summary>
public sealed record GetStaffTicketCountsQuery : IRequest<Result<StaffTicketCounts>>;

/// <summary>Open (needs a first response), unassigned (nobody has claimed it), and urgent or high priority.</summary>
public sealed record StaffTicketCounts(int Open, int Unassigned, int UrgentOrHigh);

public sealed class GetStaffTicketCountsQueryHandler(ISupportTicketRepository tickets, ICurrentUser currentUser)
    : IRequestHandler<GetStaffTicketCountsQuery, Result<StaffTicketCounts>>
{
    public async Task<Result<StaffTicketCounts>> Handle(
        GetStaffTicketCountsQuery request, CancellationToken cancellationToken)
    {
        if (!SupportAuthority.IsStaff(currentUser))
        {
            return Result.Failure<StaffTicketCounts>(SupportErrors.StaffOnly);
        }

        (int open, int unassigned, int urgentOrHigh) = await tickets.GetStaffCountsAsync(cancellationToken);

        return Result.Success(new StaffTicketCounts(open, unassigned, urgentOrHigh));
    }
}
