using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Persistence.Repositories;

public class ScheduleBlockRepository : EfRepository<ScheduleBlock>, IScheduleBlockRepository
{
    public ScheduleBlockRepository(BeautySalonDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ScheduleBlock>> GetByDateRangeAsync(
        DateOnly from, DateOnly to, Guid professionalId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Where(b => b.ProfessionalId == professionalId && b.Date >= from && b.Date <= to)
            .OrderBy(b => b.Date).ThenBy(b => b.StartTime)
            .ToListAsync(cancellationToken);
}
