using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Support;

namespace NovaLearn.Persistence.Configurations;

public sealed class SupportTicketMessageConfiguration : IEntityTypeConfiguration<SupportTicketMessage>
{
    public void Configure(EntityTypeBuilder<SupportTicketMessage> builder)
    {
        builder.ToTable("SupportTicketMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Body).HasMaxLength(SupportTicketMessage.BodyMaxLength).IsRequired();
        builder.Property(m => m.IsInternalNote).IsRequired();

        builder.Property(m => m.Version).IsRowVersion();

        builder.HasIndex(m => m.TicketId);

        // A thread is a transcript, so who wrote each line must survive their account being
        // removed — restrict rather than cascade or set-null, the same reasoning as the ticket
        // itself surviving the account that raised it.
        builder
            .HasOne(m => m.Author)
            .WithMany()
            .HasForeignKey(m => m.AuthorId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}
