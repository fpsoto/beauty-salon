using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Features.Debts;

public static class ClientDebtMappingExtensions
{
    public static ClientDebtEntryDto ToDto(this ClientDebtEntry entry) =>
        new(
            entry.Id,
            entry.ClientId,
            entry.Client is null ? string.Empty : $"{entry.Client.Name} {entry.Client.LastName}",
            entry.Type,
            entry.Amount,
            entry.Description,
            entry.EntryDate,
            entry.PaymentMethodId,
            entry.PaymentMethod?.Name,
            entry.ProfessionalId);
}
