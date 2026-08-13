using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Assessments.Common;
using NovaLearn.Domain.Assessments;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Assessments.GetAssessmentOverview;

/// <summary>
/// Every piece of assessed work the caller is responsible for, across all of their courses.
/// </summary>
public sealed record GetAssessmentOverviewQuery : IRequest<Result<AssessmentOverviewDto>>;

/// <summary>The overview as the client sees it: the rows plus the totals over them.</summary>
public sealed record AssessmentOverviewDto(
    AssessmentOverviewSummary Summary,
    IReadOnlyList<AssessmentOverviewRow> Items);

/// <summary>
/// Builds the overview, scoped by who is asking.
///
/// An administrator sees every course. Anyone else sees only the courses they own, which is the
/// same rule <see cref="AssessmentAuthority.CheckCanManage"/> applies one course at a time; doing
/// it here as a filter rather than a check means a lecturer's list simply cannot contain another
/// lecturer's work in the first place.
/// </summary>
public sealed class GetAssessmentOverviewQueryHandler(
    IAssessmentOverview overview,
    ICurrentUser currentUser,
    IDateTimeProvider dateTime)
    : IRequestHandler<GetAssessmentOverviewQuery, Result<AssessmentOverviewDto>>
{
    /// <summary>How far ahead a deadline counts as "due soon".</summary>
    private static readonly TimeSpan DueSoonWindow = TimeSpan.FromDays(7);

    public async Task<Result<AssessmentOverviewDto>> Handle(
        GetAssessmentOverviewQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId)
        {
            return Result.Failure<AssessmentOverviewDto>(AssessmentErrors.Unauthenticated);
        }

        Guid? scope = AssessmentAuthority.IsAdmin(currentUser) ? null : callerId;

        IReadOnlyList<AssessmentOverviewRow> items =
            await overview.ListAsync(scope, cancellationToken);

        return Result.Success(new AssessmentOverviewDto(Summarise(items, dateTime.UtcNow), items));
    }

    private static AssessmentOverviewSummary Summarise(
        IReadOnlyList<AssessmentOverviewRow> items, DateTimeOffset now)
    {
        DateTimeOffset horizon = now + DueSoonWindow;

        return new AssessmentOverviewSummary(
            Total: items.Count,
            Published: items.Count(i => i.Status == AssessmentStatus.Published),
            Drafts: items.Count(i => i.Status == AssessmentStatus.Draft),
            AwaitingMarking: items.Sum(i => i.AwaitingMarkingCount),

            // A draft has no deadline anyone can see, so it is never due or overdue.
            DueSoon: items.Count(i =>
                i.Status == AssessmentStatus.Published
                && i.DueAtUtc is { } due
                && due >= now
                && due <= horizon),
            Overdue: items.Count(i =>
                i.Status == AssessmentStatus.Published
                && i.DueAtUtc is { } due
                && due < now));
    }
}
