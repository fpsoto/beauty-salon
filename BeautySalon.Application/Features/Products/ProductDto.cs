namespace BeautySalon.Application.Features.Products;

public sealed record ProductDto(Guid Id, string Name, string? Description, decimal SalePrice, bool IsActive);
