using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Common.Interfaces;

public interface ISalonServiceRepository : IRepository<SalonService>
{
    Task<IReadOnlyList<SalonService>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalonService>> GetActiveAsync(CancellationToken cancellationToken = default);

    // Guards deletion: a service already used in past appointments must be deactivated,
    // not deleted, or its historical snapshots would point at a hidden catalog entry.
    Task<bool> HasAppointmentHistoryAsync(Guid serviceId, CancellationToken cancellationToken = default);
}
