using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Common.Interfaces;

public interface IWorkingHoursRepository : IRepository<WorkingHours>
{
    Task<IReadOnlyList<WorkingHours>> GetByProfessionalAsync(Guid professionalId, CancellationToken cancellationToken = default);
}
