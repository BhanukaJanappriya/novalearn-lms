using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Content;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Enrollments;
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

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<CourseModule> CourseModules => Set<CourseModule>();

    public DbSet<Lesson> Lessons => Set<Lesson>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

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
