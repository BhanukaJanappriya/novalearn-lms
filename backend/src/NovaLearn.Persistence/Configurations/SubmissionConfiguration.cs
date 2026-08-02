using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Assessments;

namespace NovaLearn.Persistence.Configurations;

public sealed class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.ToTable("Submissions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Content).HasMaxLength(20000).IsRequired();
        builder.Property(s => s.AttachmentUrl).HasMaxLength(1024);
        builder.Property(s => s.Feedback).HasMaxLength(4000);
        builder.Property(s => s.IsLate).IsRequired();

        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(s => s.Version).IsRowVersion();

        builder.HasOne(s => s.Assignment)
            .WithMany()
            .HasForeignKey(s => s.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Deleting a learner should not silently erase the marking record, so restrict.
        builder.HasOne(s => s.Student)
            .WithMany()
            .HasForeignKey(s => s.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.AssignmentId);
        builder.HasIndex(s => s.StudentId);

        // One live submission per learner per assignment. Filtered on IsDeleted so a withdrawn
        // submission can be replaced later, matching the course-code and enrolment precedent.
        builder.HasIndex(s => new { s.AssignmentId, s.StudentId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
