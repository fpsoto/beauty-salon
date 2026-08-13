using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Features.Sales;

public static class ProductSaleMappingExtensions
{
    public static ProductSaleDto ToDto(this ProductSale sale) =>
        new(
            sale.Id,
            sale.ProductId,
            sale.SnapshotProductName,
            sale.SnapshotUnitPrice,
            sale.Quantity,
            sale.Quantity * sale.SnapshotUnitPrice,
            sale.ClientId,
            sale.Client is null ? null : $"{sale.Client.Name} {sale.Client.LastName}",
            sale.PaymentMethodId,
            sale.PaymentMethod!.Name,
            sale.SaleDate,
            sale.ProfessionalId);
}
