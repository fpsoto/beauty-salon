using BeautySalon.Domain.Common;
using BeautySalon.Domain.Enums;

namespace BeautySalon.Domain.Entities;

// A single ledger of movements per client ("cuaderno de fiado") - a Charge raises
// what they owe, a Payment lowers it. The balance itself is never stored, it's
// always computed as Sum(Charge) - Sum(Payment), same convention as
// ProductSale.Total never being persisted.
public class ClientDebtEntry : AuditableEntity
{
    public Guid ClientId { get; set; }
    public Client? Client { get; set; }

    public DebtEntryType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateOnly EntryDate { get; set; }

    // Only set when Type == Payment - how the client paid down their balance.
    public Guid? PaymentMethodId { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }

    public Guid ProfessionalId { get; set; }
    public User? Professional { get; set; }
}
