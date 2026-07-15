using BeautySalon.Domain.Common;

namespace BeautySalon.Domain.Entities;

// Bridge entity that resolves "one appointment, many services". Carries a snapshot
// of the catalog service's name/price/duration at booking time, so later catalog
// changes never alter historical appointments or reports.
public class AppointmentServiceItem : AuditableEntity
{
    public Guid AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    // Kept for traceability/reporting even if the catalog service is later deactivated.
    public Guid ServiceId { get; set; }
    public SalonService? Service { get; set; }

    public required string SnapshotServiceName { get; set; }
    public decimal SnapshotPrice { get; set; }
    public int SnapshotDurationMinutes { get; set; }

    public int SortOrder { get; set; }
}
