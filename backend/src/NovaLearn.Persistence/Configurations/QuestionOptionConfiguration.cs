using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Quizzes;

namespace NovaLearn.Persistence.Configurations;

public sealed class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.ToTable("QuizQuestionOptions");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Text).HasMaxLength(1000).IsRequired();
        builder.Property(o => o.IsCorrect).IsRequired();
        builder.Property(o => o.SortOrder).IsRequired();

        builder.Property(o => o.Version).IsRowVersion();

        builder.HasIndex(o => new { o.QuestionId, o.SortOrder });

        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}
