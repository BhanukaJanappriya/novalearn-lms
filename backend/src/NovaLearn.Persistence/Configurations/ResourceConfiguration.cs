using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Resources;

namespace NovaLearn.Persistence.Configurations;

public sealed class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("Resources");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title).HasMaxLength(Resource.TitleMaxLength).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(Resource.DescriptionMaxLength);

        builder.Property(r => r.Kind).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(r => r.Url).HasMaxLength(2048);
        builder.Property(r => r.StoredFileKey).HasMaxLength(200);
        builder.Property(r => r.OriginalFileName).HasMaxLength(260);
        builder.Property(r => r.ContentType).HasMaxLength(150);
        builder.Property(r => r.YouTubeVideoId).HasMaxLength(20);

        builder.Property(r => r.Version).IsRowVersion();

        // The wall is read newest first, almost always with the soft-delete filter applied.
        builder.HasIndex(r => r.CreatedAtUtc);
        builder.HasIndex(r => r.Kind);
        builder.HasIndex(r => r.CourseId);

        // Losing a course must not take its posts with it: they fall back to the platform wall
        // rather than vanishing, which is why this is set-null and CourseId is nullable.
        builder
            .HasOne(r => r.Course)
            .WithMany()
            .HasForeignKey(r => r.CourseId)
            .OnDelete(DeleteBehavior.SetNull);

        // Posts outlive the account that made them, so removing a person never deletes material
        // other people are relying on.
        builder
            .HasOne(r => r.PostedBy)
            .WithMany()
            .HasForeignKey(r => r.PostedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
