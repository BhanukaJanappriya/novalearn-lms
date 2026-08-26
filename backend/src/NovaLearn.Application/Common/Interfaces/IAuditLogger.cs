using NovaLearn.Domain.Audit;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>
/// Write side of the audit trail. Deliberately self-contained: it persists the entry itself
/// rather than riding along on the caller's own <see cref="IUnitOfWork"/>, since not every command
/// that needs to log one (account activation and role changes go through ASP.NET Identity's own
/// <c>UserManager</c>, which commits independently) has a unit of work to ride along on in the
/// first place. A decoupled audit write is also the more defensible choice on its own terms: a
/// failed audit write should not roll back the action it describes, and vice versa.
/// </summary>
public interface IAuditLogger
{
    Task RecordAsync(
        Guid actorId,
        AuditCategory category,
        string action,
        string? details = null,
        string? entityType = null,
        Guid? entityId = null,
        CancellationToken cancellationToken = default);
}
