using BeautySalon.Domain.Enums;

namespace BeautySalon.Application.Features.Debts;

public sealed record ClientDebtEntryDto(
    Guid Id,
    Guid ClientId,
    string ClientFullName,
    DebtEntryType Type,
    decimal Amount,
    string? Description,
    DateOnly EntryDate,
    Guid? PaymentMethodId,
    string? PaymentMethodName,
    Guid ProfessionalId);
