namespace BeautySalon.Application.Features.Clients;

public sealed record ClientDto(
    Guid Id,
    string Name,
    string LastName,
    string Rut,
    string Phone,
    string? Email,
    DateOnly? DateOfBirth,
    string? Address,
    string? Notes,
    bool IsActive,
    bool IsFavorite);
