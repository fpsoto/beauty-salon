using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Common.Interfaces;

public interface IServiceCategoryRepository : IRepository<ServiceCategory>
{
    Task<IReadOnlyList<ServiceCategory>> GetActiveAsync(CancellationToken cancellationToken = default);
}
