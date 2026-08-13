using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Common.Interfaces;

public interface IProductSaleRepository : IRepository<ProductSale>
{
    Task<IReadOnlyList<ProductSale>> GetByDateRangeAsync(
        DateOnly from, DateOnly to, Guid professionalId, CancellationToken cancellationToken = default);

    // Guards Client deletion the same way IAppointmentRepository.GetByClientAsync does -
    // a client with sale history can't be soft-deleted without hiding it from that history.
    Task<IReadOnlyList<ProductSale>> GetByClientAsync(Guid clientId, CancellationToken cancellationToken = default);
}
