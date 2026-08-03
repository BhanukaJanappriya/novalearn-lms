using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Quizzes;

namespace NovaLearn.Persistence.Configurations;

public sealed class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.ToTable("Quizzes");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Title).HasMaxLength(200).IsRequired();
        builder.Property(q => q.Description).HasMaxLength(4000);
        builder.Property(q => q.ShuffleQuestions).IsRequired();

        builder.Property(q => q.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(q => q.Version).IsRowVersion();

        builder
            .HasOne(q => q.Course)
            .WithMany()
            .HasForeignKey(q => q.CourseId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Questions are exposed as a read-only collection, so map through the backing field.
        builder
            .HasMany(q => q.Questions)
            .WithOne(question => question.Quiz)
            .HasForeignKey(question => question.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Quiz.Questions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(q => q.CourseId);

        builder.HasQueryFilter(q => !q.IsDeleted);
    }
}
