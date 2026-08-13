namespace BeautySalon.Application.Features.Debts;

public sealed record CreateChargeRequest(
    Guid ClientId,
    decimal Amount,
    string? Description,
    DateOnly EntryDate,
    Guid ProfessionalId);
