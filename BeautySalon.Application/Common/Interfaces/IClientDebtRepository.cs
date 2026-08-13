using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Common.Interfaces;

public interface IClientDebtRepository : IRepository<ClientDebtEntry>
{
    // Doubles as the delete-guard query (same dual purpose as
    // IProductSaleRepository.GetByClientAsync) - a non-empty list blocks deleting the client.
    Task<IReadOnlyList<ClientDebtEntry>> GetByClientAsync(Guid clientId, CancellationToken cancellationToken = default);

    // Feeds the salon-wide "who owes me" aggregation - grouped/summed in memory by the
    // AppService, same "small dataset" reasoning as ClientRepository.SearchAsync's RUT workaround.
    Task<IReadOnlyList<ClientDebtEntry>> GetAllWithClientAsync(CancellationToken cancellationToken = default);
}
