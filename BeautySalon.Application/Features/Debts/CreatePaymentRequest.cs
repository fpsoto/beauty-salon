namespace BeautySalon.Application.Features.Debts;

public sealed record CreatePaymentRequest(
    Guid ClientId,
    decimal Amount,
    Guid PaymentMethodId,
    DateOnly EntryDate,
    Guid ProfessionalId);
