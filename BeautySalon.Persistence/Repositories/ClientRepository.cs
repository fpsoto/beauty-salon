using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Entities;
using BeautySalon.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Persistence.Repositories;

public class ClientRepository : EfRepository<Client>, IClientRepository
{
    public ClientRepository(BeautySalonDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Client>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        // Filtered in-memory instead of via SQL LIKE: EF Core cannot translate a
        // comparison against Rut (a HasConversion value object) without applying its
        // converter to the search pattern itself, which throws at runtime. The client
        // count for a single salon is small enough that this is not a real cost.
        var term = searchTerm.Trim();
        var clients = await DbSet.ToListAsync(cancellationToken);

        return clients
            .Where(c =>
                c.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                c.LastName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                c.Phone.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                c.Rut.Value.Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public Task<Client?> GetByRutAsync(string rut, CancellationToken cancellationToken = default)
    {
        var normalizedRut = Rut.Create(rut);
        return DbSet.FirstOrDefaultAsync(c => c.Rut == normalizedRut, cancellationToken);
    }

    public async Task<IReadOnlyList<Client>> GetFavoritesAsync(CancellationToken cancellationToken = default) =>
        await DbSet.Where(c => c.IsFavorite).ToListAsync(cancellationToken);
}
