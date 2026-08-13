using BeautySalon.Domain.Common;

namespace BeautySalon.Application.Features.Sales;

public interface IProductSaleAppService
{
    Task<Result<IReadOnlyList<ProductSaleDto>>> GetByDateRangeAsync(
        DateOnly from, DateOnly to, Guid professionalId, CancellationToken cancellationToken = default);
    Task<Result<ProductSaleDto>> CreateAsync(CreateProductSaleRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProductSaleDto>> UpdateAsync(Guid saleId, UpdateProductSaleRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid saleId, CancellationToken cancellationToken = default);
}
