namespace BeautySalon.Application.Features.Sales;

// The product being sold can't be changed on an edit (it drives the name/price
// snapshot) - fix a mistaken product by deleting the sale and recording a new one.
public sealed record UpdateProductSaleRequest(
    int Quantity,
    decimal UnitPrice,
    Guid? ClientId,
    Guid PaymentMethodId,
    DateOnly SaleDate);
