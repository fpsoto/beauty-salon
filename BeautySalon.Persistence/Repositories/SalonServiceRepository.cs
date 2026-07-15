using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Persistence.Repositories;

public class SalonServiceRepository : EfRepository<SalonService>, ISalonServiceRepository
{
    public SalonServiceRepository(BeautySalonDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<SalonService>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
        await DbSet.Include(s => s.Category).Where(s => s.CategoryId == categoryId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SalonService>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        await DbSet.Include(s => s.Category).Where(s => s.IsActive).ToListAsync(cancellationToken);

    public async Task<bool> HasAppointmentHistoryAsync(Guid serviceId, CancellationToken cancellationToken = default) =>
        await Context.Set<AppointmentServiceItem>().AnyAsync(i => i.ServiceId == serviceId, cancellationToken);
}
