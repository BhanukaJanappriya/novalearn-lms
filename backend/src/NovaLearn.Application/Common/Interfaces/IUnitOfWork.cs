namespace NovaLearn.Application.Common.Interfaces;

/// <summary>Commits changes tracked across repositories as a single transaction.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
