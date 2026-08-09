using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Content;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Departments;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Notifications;
using NovaLearn.Domain.Quizzes;

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

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<CourseModule> CourseModules => Set<CourseModule>();

    public DbSet<Lesson> Lessons => Set<Lesson>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<Assignment> Assignments => Set<Assignment>();

    public DbSet<Submission> Submissions => Set<Submission>();

    public DbSet<Quiz> Quizzes => Set<Quiz>();

    public DbSet<Question> QuizQuestions => Set<Question>();

    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();

    public DbSet<Notification> Notifications => Set<Notification>();

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
