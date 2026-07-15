using BeautySalon.Domain.Common;
using BeautySalon.Domain.ValueObjects;

namespace BeautySalon.Domain.Entities;

public class Client : AuditableEntity
{
    public required string Name { get; set; }
    public required string LastName { get; set; }
    public required Rut Rut { get; set; }
    public required string Phone { get; set; }
    public string? Email { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFavorite { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = [];
}
