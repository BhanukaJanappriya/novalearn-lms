using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Courses;

namespace NovaLearn.Persistence.Configurations;

public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Code).HasMaxLength(20).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(2000);
        builder.Property(c => c.Category).HasMaxLength(100).IsRequired();
        builder.Property(c => c.CoverImageUrl).HasMaxLength(512);
        builder.Property(c => c.Price).HasPrecision(18, 2);

        // Enums are stored as readable strings.
        builder.Property(c => c.Level).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        // Optimistic concurrency via PostgreSQL's xmin system column.
        builder.Property(c => c.Version).IsRowVersion();

        // Unique code, but only among non-deleted rows so a soft-deleted course's
        // code can be reused (a plain unique index would reserve it forever).
        builder.HasIndex(c => c.Code).IsUnique().HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.LecturerId);

        // Owner relationship. Optional navigation avoids a query-filter mismatch with Users.
        builder
            .HasOne(c => c.Lecturer)
            .WithMany()
            .HasForeignKey(c => c.LecturerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Soft-deleted courses are excluded from normal queries.
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
