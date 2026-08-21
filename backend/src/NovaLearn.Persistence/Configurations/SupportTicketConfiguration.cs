using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Support;

namespace NovaLearn.Persistence.Configurations;

public sealed class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
{
    public void Configure(EntityTypeBuilder<SupportTicket> builder)
    {
        builder.ToTable("SupportTickets");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Subject).HasMaxLength(SupportTicket.SubjectMaxLength).IsRequired();

        builder.Property(t => t.Category).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.Priority).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(t => t.Version).IsRowVersion();

        builder.HasIndex(t => t.SubmittedById);
        builder.HasIndex(t => t.AssignedToId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Priority);

        // Deleting an account must not erase what they raised — a ticket is a record of a real
        // conversation that happened, not disposable the way session state is.
        builder
            .HasOne(t => t.SubmittedBy)
            .WithMany()
            .HasForeignKey(t => t.SubmittedById)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Losing the assigned admin just returns the ticket to the unclaimed queue.
        builder
            .HasOne(t => t.AssignedTo)
            .WithMany()
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        // Loaded through the aggregate root; EF manages the backing field directly.
        builder
            .HasMany(t => t.Messages)
            .WithOne()
            .HasForeignKey(m => m.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(SupportTicket.Messages))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
