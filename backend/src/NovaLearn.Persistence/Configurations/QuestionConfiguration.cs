using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Quizzes;

namespace NovaLearn.Persistence.Configurations;

public sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("QuizQuestions");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Text).HasMaxLength(2000).IsRequired();
        builder.Property(q => q.Points).IsRequired();
        builder.Property(q => q.SortOrder).IsRequired();

        // Newline-separated accepted answers for short-answer questions, kept as one column
        // rather than a fourth table: the list is only ever read whole.
        builder.Property(q => q.AcceptedAnswers).HasMaxLength(2000);

        builder.Property(q => q.IsRequired).IsRequired();

        // Shown to whoever marks an essay, never to the learner.
        builder.Property(q => q.MarkingGuidance).HasMaxLength(2000);

        builder.Property(q => q.Type).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(q => q.Version).IsRowVersion();

        builder
            .HasMany(q => q.Options)
            .WithOne(o => o.Question)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Question.Options))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(q => new { q.QuizId, q.SortOrder });

        builder.HasQueryFilter(q => !q.IsDeleted);
    }
}
