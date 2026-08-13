using BeautySalon.Domain.Common;

namespace BeautySalon.Application.Features.Products;

public interface IProductAppService
{
    Task<Result<IReadOnlyList<ProductDto>>> GetAllAsync(bool onlyActive, CancellationToken cancellationToken = default);
    Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProductDto>> UpdateAsync(Guid productId, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid productId, CancellationToken cancellationToken = default);
}
