using BeautySalon.Domain.Common;

namespace BeautySalon.Application.Common.Interfaces;

// Merges Appointment and ScheduleBlock into a single list of occupied intervals to
// validate overlaps, compute free/used time and render calendar availability -
// centralizes the logic instead of duplicating it per occupant type.
public interface IScheduleAvailabilityChecker
{
    Task<Result> EnsureAvailableAsync(
        Guid professionalId, DateOnly date, TimeOnly startTime, TimeOnly endTime,
        Guid? excludeAppointmentId = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IScheduleOccupant>> GetOccupiedIntervalsAsync(
        DateOnly date, Guid professionalId, CancellationToken cancellationToken = default);
}
