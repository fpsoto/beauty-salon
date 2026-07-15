using BeautySalon.Domain.Common;

namespace BeautySalon.Domain.Entities;

// Minimal shape to unblock future product sales - no inventory/category/appointment
// bridge yet; adding those later is additive, not a rewrite.
public class Product : AuditableEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal SalePrice { get; set; }
    public bool IsActive { get; set; } = true;
}
