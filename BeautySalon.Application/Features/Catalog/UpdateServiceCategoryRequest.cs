namespace BeautySalon.Application.Features.Catalog;

public sealed record UpdateServiceCategoryRequest(string Name, string ColorHex, bool IsActive);
