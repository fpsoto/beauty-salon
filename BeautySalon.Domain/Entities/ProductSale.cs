using BeautySalon.Domain.Common;

namespace BeautySalon.Domain.Entities;

// A standalone retail sale, no appointment involved. Carries a snapshot of the
// product's name/price at sale time, same reasoning as AppointmentServiceItem -
// later catalog price changes must never alter historical sales/reports.
public class ProductSale : AuditableEntity
{
    // Kept for traceability/reporting even if the catalog product is later deactivated.
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public required string SnapshotProductName { get; set; }
    public decimal SnapshotUnitPrice { get; set; }
    public int Quantity { get; set; } = 1;

    // Walk-in sales are allowed - no client record required.
    public Guid? ClientId { get; set; }
    public Client? Client { get; set; }

    public Guid PaymentMethodId { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }

    public DateOnly SaleDate { get; set; }

    public Guid ProfessionalId { get; set; }
    public User? Professional { get; set; }
}
