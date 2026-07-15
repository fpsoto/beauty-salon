using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Common.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken cancellationToken = default);
}
