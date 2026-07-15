using BeautySalon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautySalon.Persistence.Configurations;

public class SalonServiceConfiguration : IEntityTypeConfiguration<SalonService>
{
    public void Configure(EntityTypeBuilder<SalonService> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(150);
        builder.Property(s => s.ColorHex).HasMaxLength(9);
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.Property(s => s.SuggestedPrice).HasPrecision(18, 2);
    }
}
