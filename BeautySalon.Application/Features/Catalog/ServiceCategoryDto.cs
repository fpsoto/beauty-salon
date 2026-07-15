namespace BeautySalon.Application.Features.Catalog;

public sealed record ServiceCategoryDto(Guid Id, string Name, string ColorHex, bool IsActive);
