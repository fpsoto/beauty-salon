using BeautySalon.Domain.Common;
using BeautySalon.Domain.Enums;

namespace BeautySalon.Domain.Entities;

public class User : AuditableEntity
{
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string FullName { get; set; }
    public string? Email { get; set; }
    public UserRole Role { get; set; } = UserRole.Admin;
    public bool IsActive { get; set; } = true;

    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<WorkingHours> WorkingHours { get; set; } = [];
    public ICollection<ScheduleBlock> ScheduleBlocks { get; set; } = [];
}
