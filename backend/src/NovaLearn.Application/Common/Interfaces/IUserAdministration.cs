using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>
/// Write-side port for account administration. Backed by ASP.NET Identity, so it stays out of
/// the Application layer; see <see cref="IUserDirectory"/> for the read side.
/// </summary>
public interface IUserAdministration
{
    /// <summary>Enables or disables sign-in for an account without deleting it.</summary>
    Task<Result> SetActiveAsync(Guid userId, bool isActive, CancellationToken cancellationToken);

    /// <summary>
    /// Replaces the account's roles with exactly <paramref name="roles"/>, adding and removing
    /// as needed so the call is idempotent.
    /// </summary>
    Task<Result> SetRolesAsync(Guid userId, IReadOnlyList<string> roles, CancellationToken cancellationToken);

    /// <summary>Marks the email confirmed without the user following a link.</summary>
    Task<Result> ConfirmEmailManuallyAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Clears a lockout imposed by repeated failed sign-ins.</summary>
    Task<Result> ClearLockoutAsync(Guid userId, CancellationToken cancellationToken);
}
