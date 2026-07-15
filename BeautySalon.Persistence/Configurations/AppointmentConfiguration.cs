using BeautySalon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautySalon.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Notes).HasMaxLength(2000);
        builder.Property(a => a.InternalNotes).HasMaxLength(2000);

        builder.Property(a => a.SuggestedPrice).HasPrecision(18, 2);
        builder.Property(a => a.ChargedPrice).HasPrecision(18, 2);
        builder.Property(a => a.Discount).HasPrecision(18, 2);
        builder.Property(a => a.Tip).HasPrecision(18, 2);

        builder.HasOne(a => a.Professional)
            .WithMany(u => u.Appointments)
            .HasForeignKey(a => a.ProfessionalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.PaymentMethod)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-FK for the reschedule chain - Restrict avoids a self-referencing cascade path.
        builder.HasOne(a => a.RescheduledFromAppointment)
            .WithMany()
            .HasForeignKey(a => a.RescheduledFromAppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.ServiceItems)
            .WithOne(i => i.Appointment)
            .HasForeignKey(i => i.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.ProfessionalId, a.Date });
    }
}
