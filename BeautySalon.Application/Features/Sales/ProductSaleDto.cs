namespace BeautySalon.Application.Features.Sales;

public sealed record ProductSaleDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal Total,
    Guid? ClientId,
    string? ClientFullName,
    Guid PaymentMethodId,
    string PaymentMethodName,
    DateOnly SaleDate,
    Guid ProfessionalId);
