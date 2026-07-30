using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Content;

namespace NovaLearn.Persistence.Configurations;

public sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("Lessons");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Title).HasMaxLength(200).IsRequired();
        builder.Property(l => l.ContentUrl).HasMaxLength(1024);
        builder.Property(l => l.TextContent).HasMaxLength(20000);
        builder.Property(l => l.SortOrder).IsRequired();
        builder.Property(l => l.IsPreview).IsRequired();

        // Enums are stored as readable strings.
        builder.Property(l => l.Type).HasConversion<string>().HasMaxLength(20).IsRequired();

        // Optimistic concurrency via PostgreSQL's xmin system column.
        builder.Property(l => l.Version).IsRowVersion();

        // Lessons are always read in module order, so index the pair.
        builder.HasIndex(l => new { l.ModuleId, l.SortOrder });

        // The module-to-lesson relationship (cascade) is configured on CourseModuleConfiguration.

        // Soft-deleted lessons are excluded from normal queries.
        builder.HasQueryFilter(l => !l.IsDeleted);
    }
}
