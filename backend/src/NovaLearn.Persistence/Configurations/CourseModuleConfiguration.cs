using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Content;

namespace NovaLearn.Persistence.Configurations;

public sealed class CourseModuleConfiguration : IEntityTypeConfiguration<CourseModule>
{
    public void Configure(EntityTypeBuilder<CourseModule> builder)
    {
        builder.ToTable("CourseModules");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Description).HasMaxLength(2000);
        builder.Property(m => m.SortOrder).IsRequired();

        // Optimistic concurrency via PostgreSQL's xmin system column.
        builder.Property(m => m.Version).IsRowVersion();

        // Modules are always read in course order, so index the pair.
        builder.HasIndex(m => new { m.CourseId, m.SortOrder });

        // Deleting a course takes its modules with it.
        builder
            .HasOne(m => m.Course)
            .WithMany()
            .HasForeignKey(m => m.CourseId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Lessons are exposed as a read-only collection, so map through the backing field.
        builder
            .HasMany(m => m.Lessons)
            .WithOne(l => l.Module)
            .HasForeignKey(l => l.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(CourseModule.Lessons))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Soft-deleted modules are excluded from normal queries.
        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}
