using BeautySalon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeautySalon.Persistence.Configurations;

public class AppSettingsConfiguration : IEntityTypeConfiguration<AppSettings>
{
    public void Configure(EntityTypeBuilder<AppSettings> builder)
    {
        builder.Property(s => s.Language).IsRequired().HasMaxLength(5);
        builder.Property(s => s.Theme).HasConversion<string>().HasMaxLength(10);
        builder.Property(s => s.Currency).HasConversion<string>().HasMaxLength(5);
        builder.Property(s => s.DateFormat).IsRequired().HasMaxLength(20);
        builder.Property(s => s.TimeFormat).IsRequired().HasMaxLength(20);

        builder.HasMany(s => s.NotificationRules)
            .WithOne(r => r.AppSettings)
            .HasForeignKey(r => r.AppSettingsId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
