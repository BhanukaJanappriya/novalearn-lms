using NovaLearn.Application.Common.Models;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>
/// Port over ASP.NET Identity. Keeps <c>UserManager</c>/<c>SignInManager</c> out of the
/// Application layer so use cases stay testable and framework-agnostic.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Creates a user with the default role and an unconfirmed email. Returns the created
    /// user, or a <see cref="ErrorType.Conflict"/>/<see cref="ErrorType.Validation"/> error.
    /// </summary>
    Task<Result<AuthenticatedUser>> CreateUserAsync(
        string email, string password, string firstName, string lastName, CancellationToken cancellationToken);

    /// <summary>
    /// Verifies credentials and account state (exists, active, not locked out, email confirmed).
    /// Returns a generic invalid-credentials error where appropriate to avoid user enumeration.
    /// </summary>
    Task<Result<AuthenticatedUser>> ValidateCredentialsAsync(
        string email, string password, CancellationToken cancellationToken);

    Task<Result<AuthenticatedUser>> FindByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<string> GenerateEmailConfirmationTokenAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken);

    Task RecordSuccessfulLoginAsync(Guid userId, CancellationToken cancellationToken);
}
