using BeautySalon.Domain.Entities;
using BeautySalon.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautySalon.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.LastName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Phone).IsRequired().HasMaxLength(30);
        builder.Property(c => c.Email).HasMaxLength(200);
        builder.Property(c => c.Address).HasMaxLength(300);
        builder.Property(c => c.Notes).HasMaxLength(2000);

        // Rut is a value object with its own check-digit invariant; stored as its
        // normalized string form and kept unique at the database level.
        builder.Property(c => c.Rut)
            .HasConversion(rut => rut.Value, value => Rut.Create(value))
            .IsRequired()
            .HasMaxLength(15);
        builder.HasIndex(c => c.Rut).IsUnique();

        builder.HasMany(c => c.Appointments)
            .WithOne(a => a.Client)
            .HasForeignKey(a => a.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
