using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Common.Interfaces;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<IReadOnlyList<Appointment>> GetByDateRangeAsync(DateOnly from, DateOnly to, Guid professionalId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetByClientAsync(Guid clientId, CancellationToken cancellationToken = default);

    // Filtered server-side rather than loading every appointment ever (all statuses) and
    // filtering in memory - used by reports' "last completed visit per client" calculation,
    // which needs the full history (not just the report's date range).
    Task<IReadOnlyList<Appointment>> GetCompletedAsync(CancellationToken cancellationToken = default);

    // excludeAppointmentId lets an appointment being edited skip flagging itself as an overlap.
    Task<bool> HasOverlapAsync(Guid professionalId, DateOnly date, TimeOnly startTime, TimeOnly endTime, Guid? excludeAppointmentId = null, CancellationToken cancellationToken = default);
}
