using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Persistence.Repositories;

public class ProductSaleRepository : EfRepository<ProductSale>, IProductSaleRepository
{
    public ProductSaleRepository(BeautySalonDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ProductSale>> GetByDateRangeAsync(
        DateOnly from, DateOnly to, Guid professionalId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(s => s.Product)
            .Include(s => s.Client)
            .Include(s => s.PaymentMethod)
            .Where(s => s.ProfessionalId == professionalId && s.SaleDate >= from && s.SaleDate <= to)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ProductSale>> GetByClientAsync(Guid clientId, CancellationToken cancellationToken = default) =>
        await DbSet.Where(s => s.ClientId == clientId).ToListAsync(cancellationToken);
}
