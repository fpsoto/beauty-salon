namespace BeautySalon.Application.Features.Catalog;

public sealed record CreateSalonServiceRequest(
    string Name,
    Guid CategoryId,
    decimal SuggestedPrice,
    int DurationMinutes,
    string? ColorHex,
    string? Description);
