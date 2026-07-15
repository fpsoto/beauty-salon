using BeautySalon.Domain.Common;

namespace BeautySalon.Domain.Entities;

public class PaymentMethod : AuditableEntity
{
    public required string Name { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = [];
}
