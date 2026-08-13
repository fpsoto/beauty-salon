using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Persistence.Repositories;

public class ClientDebtRepository : EfRepository<ClientDebtEntry>, IClientDebtRepository
{
    public ClientDebtRepository(BeautySalonDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ClientDebtEntry>> GetByClientAsync(Guid clientId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Where(e => e.ClientId == clientId)
            .OrderBy(e => e.EntryDate)
            .ThenBy(e => e.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ClientDebtEntry>> GetAllWithClientAsync(CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(e => e.Client)
            .ToListAsync(cancellationToken);
}
