using BeautySalon.Domain.Common;
using BeautySalon.Domain.Enums;

namespace BeautySalon.Domain.Entities;

public class Appointment : AuditableEntity, IScheduleOccupant
{
    public Guid ClientId { get; set; }
    public Client? Client { get; set; }

    public Guid ProfessionalId { get; set; }
    public User? Professional { get; set; }

    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }

    // Calculated as the sum of all ServiceItems' snapshot durations and persisted
    // (not recomputed via join on every query) so date-range reads stay cheap.
    public TimeOnly EndTime { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
    public string? Notes { get; set; }

    public decimal SuggestedPrice { get; set; }
    public decimal? ChargedPrice { get; set; }
    public decimal? Discount { get; set; }
    public decimal? Tip { get; set; }

    public Guid? PaymentMethodId { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }

    public string? InternalNotes { get; set; }

    // Self-FK: when rescheduled, the new appointment points back to the one it
    // replaced, keeping a traceable chain for reporting (reschedule history).
    public Guid? RescheduledFromAppointmentId { get; set; }
    public Appointment? RescheduledFromAppointment { get; set; }

    public ICollection<AppointmentServiceItem> ServiceItems { get; set; } = [];
}
