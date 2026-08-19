using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Payments;

namespace NovaLearn.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.CourseTitle).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Amount).HasPrecision(10, 2).IsRequired();
        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.ProviderCheckoutSessionId).HasMaxLength(200).IsRequired();
        builder.Property(p => p.ProviderPaymentIntentId).HasMaxLength(200);
        builder.Property(p => p.RefundedAmount).HasPrecision(10, 2);
        builder.Property(p => p.FailureReason).HasMaxLength(500);

        builder.Property(p => p.Version).IsRowVersion();

        // How a webhook finds the payment it is settling.
        builder.HasIndex(p => p.ProviderCheckoutSessionId).IsUnique();

        builder.HasIndex(p => p.StudentId);
        builder.HasIndex(p => p.CourseId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CreatedAtUtc);

        // A student's account may be deleted without erasing what they were charged.
        builder
            .HasOne(p => p.Student)
            .WithMany()
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Financial history has to outlive the course it was for, unlike an enrolment, which is
        // allowed to go with it. Restrict rather than the Cascade Enrollment uses for its own
        // course FK.
        builder
            .HasOne(p => p.Course)
            .WithMany()
            .HasForeignKey(p => p.CourseId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .HasOne(p => p.Enrollment)
            .WithMany()
            .HasForeignKey(p => p.EnrollmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
