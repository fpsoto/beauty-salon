using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Common;

namespace BeautySalon.Application.Features.Schedule;

// Merges Appointment and ScheduleBlock into a single occupied-interval view so
// overlap checks, free/used time and calendar rendering share one implementation.
public sealed class ScheduleAvailabilityChecker : IScheduleAvailabilityChecker
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWorkingHoursProvider _workingHoursProvider;

    public ScheduleAvailabilityChecker(IUnitOfWork unitOfWork, IWorkingHoursProvider workingHoursProvider)
    {
        _unitOfWork = unitOfWork;
        _workingHoursProvider = workingHoursProvider;
    }

    public async Task<Result> EnsureAvailableAsync(
        Guid professionalId, DateOnly date, TimeOnly startTime, TimeOnly endTime,
        Guid? excludeAppointmentId = null, CancellationToken cancellationToken = default)
    {
        var (workStart, workEnd, isWorkingDay) = await _workingHoursProvider.GetEffectiveHoursAsync(date, professionalId, cancellationToken);

        if (!isWorkingDay)
            return Result.Failure(Error.Conflict("Schedule.NonWorkingDay", "Ese día no es un día laboral."));

        if (startTime < workStart || endTime > workEnd)
            return Result.Failure(Error.Conflict("Schedule.OutsideWorkingHours", "El horario está fuera del horario laboral."));

        var hasAppointmentOverlap = await _unitOfWork.Appointments.HasOverlapAsync(
            professionalId, date, startTime, endTime, excludeAppointmentId, cancellationToken);
        if (hasAppointmentOverlap)
            return Result.Failure(Error.Conflict("Schedule.Overlap", "Ya existe una cita en ese horario."));

        var blocks = await _unitOfWork.ScheduleBlocks.GetByDateRangeAsync(date, date, professionalId, cancellationToken);
        var hasBlockOverlap = blocks.Any(b => b.StartTime < endTime && startTime < b.EndTime);
        if (hasBlockOverlap)
            return Result.Failure(Error.Conflict("Schedule.Blocked", "Ese horario está bloqueado."));

        return Result.Success();
    }

    public async Task<IReadOnlyList<IScheduleOccupant>> GetOccupiedIntervalsAsync(
        DateOnly date, Guid professionalId, CancellationToken cancellationToken = default)
    {
        var appointments = await _unitOfWork.Appointments.GetByDateRangeAsync(date, date, professionalId, cancellationToken);
        var blocks = await _unitOfWork.ScheduleBlocks.GetByDateRangeAsync(date, date, professionalId, cancellationToken);

        return appointments
            .Cast<IScheduleOccupant>()
            .Concat(blocks)
            .OrderBy(o => o.StartTime)
            .ToList();
    }
}
