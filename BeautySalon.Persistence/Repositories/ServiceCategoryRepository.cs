using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Persistence.Repositories;

public class ServiceCategoryRepository : EfRepository<ServiceCategory>, IServiceCategoryRepository
{
    public ServiceCategoryRepository(BeautySalonDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ServiceCategory>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        await DbSet.Where(c => c.IsActive).ToListAsync(cancellationToken);

    public Task<ServiceCategory?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower(), cancellationToken);
}
