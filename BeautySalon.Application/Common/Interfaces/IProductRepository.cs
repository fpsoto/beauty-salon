using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Common.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken cancellationToken = default);

    // Guards deletion: a product already sold must be deactivated, not deleted, or
    // historical sales/reports would lose its label.
    Task<bool> HasSaleHistoryAsync(Guid productId, CancellationToken cancellationToken = default);

    // Global and case-insensitive.
    Task<Product?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
