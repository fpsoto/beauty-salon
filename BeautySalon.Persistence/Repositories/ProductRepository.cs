using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Persistence.Repositories;

public class ProductRepository : EfRepository<Product>, IProductRepository
{
    public ProductRepository(BeautySalonDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        await DbSet.Where(p => p.IsActive).ToListAsync(cancellationToken);
}
