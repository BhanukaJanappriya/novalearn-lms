using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLearn.Domain.Departments;

namespace NovaLearn.Persistence.Configurations;

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name).HasMaxLength(150).IsRequired();
        builder.Property(d => d.Code).HasMaxLength(20).IsRequired();
        builder.Property(d => d.Description).HasMaxLength(1000);
        builder.Property(d => d.IsActive).IsRequired();

        builder.Property(d => d.Version).IsRowVersion();

        // Unique code among live rows only, so a retired department's code can be reused. Same
        // partial-index precedent as course codes.
        builder.HasIndex(d => d.Code).IsUnique().HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(d => d.IsActive);

        // Naming a head must never be able to delete a person, and losing a head must not delete
        // the department, so this is set-null on the way out.
        builder
            .HasOne(d => d.Head)
            .WithMany()
            .HasForeignKey(d => d.HeadId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
