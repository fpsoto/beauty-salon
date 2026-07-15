using BeautySalon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautySalon.Persistence.Configurations;

public class ScheduleBlockConfiguration : IEntityTypeConfiguration<ScheduleBlock>
{
    public void Configure(EntityTypeBuilder<ScheduleBlock> builder)
    {
        builder.Property(b => b.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.Reason).HasMaxLength(500);

        builder.HasOne(b => b.Professional)
            .WithMany(u => u.ScheduleBlocks)
            .HasForeignKey(b => b.ProfessionalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.ProfessionalId, b.Date });
    }
}
