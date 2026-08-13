namespace BeautySalon.Application.Features.Products;

public sealed record CreateProductRequest(string Name, string? Description, decimal SalePrice);
