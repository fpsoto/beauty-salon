using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Features.Products;

public static class ProductMappingExtensions
{
    public static ProductDto ToDto(this Product product) =>
        new(product.Id, product.Name, product.Description, product.SalePrice, product.IsActive);
}
