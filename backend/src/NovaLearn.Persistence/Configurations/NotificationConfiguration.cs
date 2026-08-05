using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Notifications;

namespace NovaLearn.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Message).HasMaxLength(1000).IsRequired();
        builder.Property(n => n.Link).HasMaxLength(500);
        builder.Property(n => n.IsRead).IsRequired();

        builder.Property(n => n.Type).HasConversion<string>().HasMaxLength(40).IsRequired();

        builder.Property(n => n.Version).IsRowVersion();

        // Deleting a person should not orphan their feed rows silently.
        builder
            .HasOne(n => n.Recipient)
            .WithMany()
            .HasForeignKey(n => n.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);

        // The feed is always read newest first for one person, and the unread badge filters on
        // IsRead, so index the triple the queries actually use.
        builder.HasIndex(n => new { n.RecipientId, n.IsRead, n.CreatedAtUtc });

        builder.HasQueryFilter(n => !n.IsDeleted);
    }
}
