using BeautySalon.Domain.Common;

namespace BeautySalon.Domain.Entities;

public class ServiceCategory : AuditableEntity
{
    public required string Name { get; set; }
    public required string ColorHex { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<SalonService> Services { get; set; } = [];
}
