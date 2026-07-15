namespace BeautySalon.Application.Common.Interfaces;

// Resolves the effective working hours for a professional on a given date. Today it
// only reads WorkingHours; adding holiday exceptions later only changes the
// implementation, never this contract or its consumers.
public interface IWorkingHoursProvider
{
    Task<(TimeOnly StartTime, TimeOnly EndTime, bool IsWorkingDay)> GetEffectiveHoursAsync(
        DateOnly date, Guid professionalId, CancellationToken cancellationToken = default);
}
