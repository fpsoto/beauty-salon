using BeautySalon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautySalon.Persistence.Configurations;

public class AppointmentServiceItemConfiguration : IEntityTypeConfiguration<AppointmentServiceItem>
{
    public void Configure(EntityTypeBuilder<AppointmentServiceItem> builder)
    {
        builder.Property(i => i.SnapshotServiceName).IsRequired().HasMaxLength(150);
        builder.Property(i => i.SnapshotPrice).HasPrecision(18, 2);

        builder.HasOne(i => i.Service)
            .WithMany(s => s.AppointmentServiceItems)
            .HasForeignKey(i => i.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
