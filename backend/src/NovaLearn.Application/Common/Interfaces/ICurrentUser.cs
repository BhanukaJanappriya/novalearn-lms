namespace NovaLearn.Application.Common.Interfaces;

/// <summary>Ambient accessor for the caller behind the current request (for auditing/authorization).</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }

    string? Email { get; }

    bool IsAuthenticated { get; }

    /// <summary>Best-effort client IP, used to stamp issued/revoked refresh tokens.</summary>
    string? IpAddress { get; }

    /// <summary>Whether the current caller holds the given role.</summary>
    bool IsInRole(string role);
}
