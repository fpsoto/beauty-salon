using BeautySalon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautySalon.Persistence.Configurations;

public class WorkingHoursConfiguration : IEntityTypeConfiguration<WorkingHours>
{
    public void Configure(EntityTypeBuilder<WorkingHours> builder)
    {
        builder.HasOne(w => w.Professional)
            .WithMany(u => u.WorkingHours)
            .HasForeignKey(w => w.ProfessionalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(w => new { w.ProfessionalId, w.DayOfWeek }).IsUnique();
    }
}
