using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Enrollments;

namespace NovaLearn.Persistence.Configurations;

public sealed class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EnrolledAtUtc).IsRequired();
        builder.Property(e => e.ProgressPercent).HasDefaultValue(0).IsRequired();

        // Enums are stored as readable strings.
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        // Optimistic concurrency via PostgreSQL's xmin system column.
        builder.Property(e => e.Version).IsRowVersion();

        // A student can hold only one live enrolment per course, but the filter on non-deleted
        // rows means a dropped-and-soft-deleted enrolment can be recreated later. This mirrors
        // the CourseCodeFilteredUnique precedent.
        builder
            .HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(e => e.CourseId);
        builder.HasIndex(e => e.StudentId);
        builder.HasIndex(e => e.Status);

        // Student relationship. Optional navigation avoids a query-filter mismatch with Users.
        builder
            .HasOne(e => e.Student)
            .WithMany()
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Deleting a course takes its enrolments with it.
        builder
            .HasOne(e => e.Course)
            .WithMany()
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Soft-deleted enrolments are excluded from normal queries.
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
