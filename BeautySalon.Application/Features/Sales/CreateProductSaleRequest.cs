namespace BeautySalon.Application.Features.Sales;

public sealed record CreateProductSaleRequest(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    Guid? ClientId,
    Guid PaymentMethodId,
    DateOnly SaleDate,
    Guid ProfessionalId);
