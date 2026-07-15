using BeautySalon.Domain.Common;

namespace BeautySalon.Domain.Entities;

// One row per (ProfessionalId, DayOfWeek). Read through IWorkingHoursProvider so a
// future WorkingHoursException (holidays) can override this without touching callers.
public class WorkingHours : AuditableEntity
{
    public Guid ProfessionalId { get; set; }
    public User? Professional { get; set; }

    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsWorkingDay { get; set; } = true;
}
