using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Features.Catalog;

public static class CatalogMappingExtensions
{
    public static ServiceCategoryDto ToDto(this ServiceCategory category) =>
        new(category.Id, category.Name, category.ColorHex, category.IsActive);

    // categoryNameOverride covers callers where Category wasn't (re)loaded after a
    // write, e.g. right after changing CategoryId on an update.
    public static SalonServiceDto ToDto(this SalonService service, string? categoryNameOverride = null) => new(
        service.Id,
        service.Name,
        service.CategoryId,
        categoryNameOverride ?? service.Category?.Name ?? string.Empty,
        service.SuggestedPrice,
        service.DurationMinutes,
        service.ColorHex,
        service.IsActive,
        service.Description);
}
