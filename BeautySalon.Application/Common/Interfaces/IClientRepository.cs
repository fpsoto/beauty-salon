using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Common.Interfaces;

public interface IClientRepository : IRepository<Client>
{
    // Backs the real-time search by name, phone or RUT.
    Task<IReadOnlyList<Client>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<Client?> GetByRutAsync(string rut, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Client>> GetFavoritesAsync(CancellationToken cancellationToken = default);
}
