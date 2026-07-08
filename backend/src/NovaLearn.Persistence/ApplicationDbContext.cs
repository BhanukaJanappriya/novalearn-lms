using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Persistence;

/// <summary>
/// The EF Core unit of work for NovaLearn. Extends the Identity schema and adds the domain's
/// own aggregates. Also satisfies <see cref="IUnitOfWork"/> — <c>SaveChangesAsync</c> is provided
/// by <see cref="DbContext"/> itself.
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IUnitOfWork
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all IEntityTypeConfiguration<T> in this assembly (identity + domain tables).
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Give the Identity tables friendly, snake-free but readable names.
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
    }
}
