using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Quizzes;

namespace NovaLearn.Persistence.Configurations;

public sealed class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
{
    public void Configure(EntityTypeBuilder<QuizAttempt> builder)
    {
        builder.ToTable("QuizAttempts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.AttemptNumber).IsRequired();
        builder.Property(a => a.ScorePercent).IsRequired();
        builder.Property(a => a.WasLate).IsRequired();

        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(a => a.Version).IsRowVersion();

        builder
            .HasOne(a => a.Quiz)
            .WithMany()
            .HasForeignKey(a => a.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        // Deleting a learner should not silently erase their results, so restrict.
        builder
            .HasOne(a => a.Student)
            .WithMany()
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(a => a.Answers)
            .WithOne(answer => answer.Attempt)
            .HasForeignKey(answer => answer.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(QuizAttempt.Answers))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Attempts are read per learner per quiz, and per quiz for the results roster.
        builder.HasIndex(a => new { a.QuizId, a.StudentId });
        builder.HasIndex(a => a.StudentId);

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
