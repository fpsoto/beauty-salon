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

    public async Task<bool> HasSaleHistoryAsync(Guid productId, CancellationToken cancellationToken = default) =>
        await Context.Set<ProductSale>().AnyAsync(s => s.ProductId == productId, cancellationToken);

    public Task<Product?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower(), cancellationToken);
}
