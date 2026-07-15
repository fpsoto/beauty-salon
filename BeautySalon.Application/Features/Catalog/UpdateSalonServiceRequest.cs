namespace BeautySalon.Application.Features.Catalog;

public sealed record UpdateSalonServiceRequest(
    string Name,
    Guid CategoryId,
    decimal SuggestedPrice,
    int DurationMinutes,
    string? ColorHex,
    string? Description,
    bool IsActive);
