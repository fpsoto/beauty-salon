namespace BeautySalon.Application.Features.Clients;

public sealed record CreateClientRequest(
    string Name,
    string LastName,
    string Rut,
    string Phone,
    string? Email,
    DateOnly? DateOfBirth,
    string? Address,
    string? Notes);
