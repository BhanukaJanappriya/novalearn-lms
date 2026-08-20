using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Settings;

namespace NovaLearn.Persistence.Configurations;

public sealed class PlatformSettingsConfiguration : IEntityTypeConfiguration<PlatformSettings>
{
    public void Configure(EntityTypeBuilder<PlatformSettings> builder)
    {
        builder.ToTable("PlatformSettings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SiteName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.SupportEmail).HasMaxLength(320).IsRequired();
        builder.Property(s => s.MaintenanceMessage).HasMaxLength(500);
        builder.Property(s => s.DefaultCurrency).HasMaxLength(3).IsRequired();

        builder.Property(s => s.Version).IsRowVersion();

        // Deliberately no soft-delete query filter, unlike every other aggregate here. There is no
        // delete use case for this row, and it must always be findable at its fixed id regardless
        // of IsDeleted — a config singleton silently vanishing because a bug somewhere set that
        // flag would be a far worse failure than the flag simply being ignored.
    }
}
