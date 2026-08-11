using NovaLearn.Application.Features.Profile.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>
/// Self-service profile edits, backed by ASP.NET Identity.
///
/// Every method takes the user id from the caller's token rather than from a request body, so
/// there is no id for a caller to swap. That is the whole ownership guarantee: it is structural
/// rather than a check that could be forgotten.
/// </summary>
public interface IProfileService
{
    Task<MyProfileDto?> GetAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Sets or clears the caller's picture. A null url removes it.</summary>
    Task<Result> SetAvatarAsync(Guid userId, string? avatarUrl, CancellationToken cancellationToken);
}
