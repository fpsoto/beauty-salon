namespace BeautySalon.Application.Features.Catalog;

public sealed record SalonServiceDto(
    Guid Id,
    string Name,
    Guid CategoryId,
    string CategoryName,
    decimal SuggestedPrice,
    int DurationMinutes,
    string? ColorHex,
    bool IsActive,
    string? Description);
