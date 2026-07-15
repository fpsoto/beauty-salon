using BeautySalon.Application.Common.Interfaces;

namespace BeautySalon.Application.Features.Schedule;

// Today only reads WorkingHours; adding a WorkingHoursException (holidays) later
// only changes this implementation, never IWorkingHoursProvider's consumers.
public sealed class WorkingHoursProvider : IWorkingHoursProvider
{
    private readonly IUnitOfWork _unitOfWork;

    public WorkingHoursProvider(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(TimeOnly StartTime, TimeOnly EndTime, bool IsWorkingDay)> GetEffectiveHoursAsync(
        DateOnly date, Guid professionalId, CancellationToken cancellationToken = default)
    {
        var hours = await _unitOfWork.WorkingHours.GetByProfessionalAsync(professionalId, cancellationToken);
        var match = hours.FirstOrDefault(h => h.DayOfWeek == date.DayOfWeek);

        return match is { IsWorkingDay: true }
            ? (match.StartTime, match.EndTime, true)
            : (TimeOnly.MinValue, TimeOnly.MinValue, false);
    }
}
