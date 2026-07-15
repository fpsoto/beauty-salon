using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Common.Interfaces;

public interface IScheduleBlockRepository : IRepository<ScheduleBlock>
{
    Task<IReadOnlyList<ScheduleBlock>> GetByDateRangeAsync(DateOnly from, DateOnly to, Guid professionalId, CancellationToken cancellationToken = default);
}
