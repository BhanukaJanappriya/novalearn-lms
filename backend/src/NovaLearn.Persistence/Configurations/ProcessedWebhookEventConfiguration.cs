using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Payments;

namespace NovaLearn.Persistence.Configurations;

public sealed class ProcessedWebhookEventConfiguration : IEntityTypeConfiguration<ProcessedWebhookEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedWebhookEvent> builder)
    {
        builder.ToTable("ProcessedWebhookEvents");

        // Stripe's own event id is the whole record, and the natural key: the table exists only
        // to answer "have I seen this id before".
        builder.HasKey(e => e.ProviderEventId);
        builder.Property(e => e.ProviderEventId).HasMaxLength(255);

        builder.Property(e => e.ProcessedAtUtc).IsRequired();
    }
}
