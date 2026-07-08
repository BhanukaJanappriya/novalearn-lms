using NovaLearn.Domain.Common;

namespace NovaLearn.Domain.Identity.Events;

/// <summary>
/// Raised when a new account is created. Handlers may send the verification email,
/// notify administrators of a pending approval, or seed a default profile.
/// </summary>
public sealed record UserRegisteredDomainEvent(Guid UserId, string Email, string FullName) : DomainEvent;
