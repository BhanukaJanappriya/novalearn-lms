using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Reports;

namespace NovaLearn.Persistence.Configurations;

public sealed class ReportRunConfiguration : IEntityTypeConfiguration<ReportRun>
{
    public void Configure(EntityTypeBuilder<ReportRun> builder)
    {
        builder.ToTable("ReportRuns");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Type).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(r => r.FiltersSummary).HasMaxLength(500);

        builder.Property(r => r.Version).IsRowVersion();

        builder.HasIndex(r => r.CreatedAtUtc);
        builder.HasIndex(r => r.Type);

        // An audit record must outlive the account that generated it.
        builder
            .HasOne(r => r.GeneratedBy)
            .WithMany()
            .HasForeignKey(r => r.GeneratedById)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
