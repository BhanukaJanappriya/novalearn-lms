using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Audit;

namespace NovaLearn.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Category).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(AuditLog.ActionMaxLength).IsRequired();
        builder.Property(a => a.Details).HasMaxLength(AuditLog.DetailsMaxLength);
        builder.Property(a => a.EntityType).HasMaxLength(100);

        builder.Property(a => a.Version).IsRowVersion();

        builder.HasIndex(a => a.CreatedAtUtc);
        builder.HasIndex(a => a.Category);
        builder.HasIndex(a => a.ActorId);

        // An audit record must outlive the account that generated it.
        builder
            .HasOne(a => a.Actor)
            .WithMany()
            .HasForeignKey(a => a.ActorId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
