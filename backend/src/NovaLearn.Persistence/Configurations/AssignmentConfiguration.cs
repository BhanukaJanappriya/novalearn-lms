using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Assessments;

namespace NovaLearn.Persistence.Configurations;

public sealed class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("Assignments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Instructions).HasMaxLength(20000);
        builder.Property(a => a.MaxPoints).IsRequired();
        builder.Property(a => a.AllowLateSubmissions).IsRequired();

        // Enums are stored as readable strings.
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        // Optimistic concurrency via PostgreSQL's xmin system column.
        builder.Property(a => a.Version).IsRowVersion();

        builder.HasOne(a => a.Course)
            .WithMany()
            .HasForeignKey(a => a.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Assignments are always listed per course, usually by due date.
        builder.HasIndex(a => new { a.CourseId, a.DueAtUtc });

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
