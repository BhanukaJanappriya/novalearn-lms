using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Quizzes;

namespace NovaLearn.Persistence.Configurations;

public sealed class AttemptAnswerConfiguration : IEntityTypeConfiguration<AttemptAnswer>
{
    public void Configure(EntityTypeBuilder<AttemptAnswer> builder)
    {
        builder.ToTable("QuizAttemptAnswers");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.TextAnswer).HasMaxLength(2000);
        builder.Property(a => a.PointsAwarded).IsRequired();

        builder.Property(a => a.Version).IsRowVersion();

        // Restrict, not cascade: deleting a question out from under a marked attempt would
        // silently change a learner's recorded result.
        builder
            .HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        // One answer per question per attempt.
        builder.HasIndex(a => new { a.AttemptId, a.QuestionId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
