using MediatR;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Admin.Directory;

/// <summary>Which population the directory is being asked for.</summary>
public enum DirectoryAudience
{
    Students,
    TeachingStaff,
}

/// <summary>The people directory: who is on the platform, and how they are doing.</summary>
public sealed record GetDirectoryQuery(DirectoryAudience Audience, string? Search)
    : IRequest<Result<IReadOnlyList<DirectoryEntryDto>>>;

/// <summary>
/// A directory row. Carries no account-security state and no individual academic record; see
/// <see cref="DirectoryEntry"/> for why.
/// </summary>
public sealed record DirectoryEntryDto(
    Guid Id,
    string FullName,
    string FirstName,
    string LastName,
    string Email,
    string? AvatarUrl,
    bool IsActive,
    DateTimeOffset JoinedAtUtc,
    DateTimeOffset? LastActiveAtUtc,
    IReadOnlyList<string> Roles,
    DirectoryLearnerStats? Learner,
    DirectoryTeacherStats? Teacher)
{
    public static DirectoryEntryDto FromEntry(DirectoryEntry entry) => new(
        entry.Id,
        entry.FullName,
        entry.FirstName,
        entry.LastName,
        entry.Email,
        entry.AvatarUrl,
        entry.IsActive,
        entry.JoinedAtUtc,
        entry.LastActiveAtUtc,
        entry.Roles,
        entry.Learner,
        entry.Teacher);
}

public sealed class GetDirectoryQueryHandler(IPeopleDirectory directory)
    : IRequestHandler<GetDirectoryQuery, Result<IReadOnlyList<DirectoryEntryDto>>>
{
    public async Task<Result<IReadOnlyList<DirectoryEntryDto>>> Handle(
        GetDirectoryQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<DirectoryEntry> people = request.Audience switch
        {
            DirectoryAudience.TeachingStaff =>
                await directory.ListTeachingStaffAsync(request.Search, cancellationToken),
            _ => await directory.ListStudentsAsync(request.Search, cancellationToken),
        };

        return people.Select(DirectoryEntryDto.FromEntry).ToList();
    }
}
