using BeautySalon.Domain.Common;
using BeautySalon.Domain.Enums;

namespace BeautySalon.Domain.Entities;

// Kept as its own entity (not merged into Appointment) since it has a different
// data shape (no client/services/price); it implements IScheduleOccupant so
// availability calculations can merge it with Appointment into one occupied list.
public class ScheduleBlock : AuditableEntity, IScheduleOccupant
{
    public Guid ProfessionalId { get; set; }
    public User? Professional { get; set; }

    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public ScheduleBlockType Type { get; set; }
    public string? Reason { get; set; }
}
