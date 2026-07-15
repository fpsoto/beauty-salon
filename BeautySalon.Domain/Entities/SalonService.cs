using BeautySalon.Domain.Common;

namespace BeautySalon.Domain.Entities;

// Named "SalonService" (not "Service") to avoid clashing with the "...AppService"
// naming used for application services one layer up.
public class SalonService : AuditableEntity
{
    public required string Name { get; set; }

    public Guid CategoryId { get; set; }
    public ServiceCategory? Category { get; set; }

    public decimal SuggestedPrice { get; set; }
    public int DurationMinutes { get; set; }
    public string? ColorHex { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }

    public ICollection<AppointmentServiceItem> AppointmentServiceItems { get; set; } = [];
}
