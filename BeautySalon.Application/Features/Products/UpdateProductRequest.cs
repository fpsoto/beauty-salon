namespace BeautySalon.Application.Features.Products;

public sealed record UpdateProductRequest(string Name, string? Description, decimal SalePrice, bool IsActive);
