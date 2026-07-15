namespace BeautySalon.Domain.Common;

// Shared shape for anything that occupies a slot on the calendar - both Appointment
// and ScheduleBlock implement it so overlap-checking can be computed against a
// single merged list of occupied intervals instead of duplicating the logic per type.
public interface IScheduleOccupant
{
    Guid Id { get; }
    Guid ProfessionalId { get; }
    DateOnly Date { get; }
    TimeOnly StartTime { get; }
    TimeOnly EndTime { get; }
}
