using Microsoft.AspNetCore.Identity;
using NovaLearn.Domain.Common;

namespace NovaLearn.Domain.Identity;

/// <summary>
/// The application user aggregate. Extends ASP.NET Identity's <see cref="IdentityUser{Guid}"/>
/// with profile, auditing, soft-delete and refresh-token state. See ADR-0002 for why the
/// identity type lives in the Domain layer.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>, IAuditable, ISoftDeletable, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private readonly List<RefreshToken> _refreshTokens = [];

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public string? AvatarUrl { get; set; }

    /// <summary>When false the account cannot authenticate, regardless of credentials.</summary>
    public bool IsActive { get; set; } = true;

    public DateTimeOffset? LastLoginAtUtc { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    // --- Auditing / soft delete ---
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    // --- Domain events ---
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>Adds a freshly issued refresh token to the user's token set.</summary>
    public void AddRefreshToken(RefreshToken token) => _refreshTokens.Add(token);
}
