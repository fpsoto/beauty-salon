using BeautySalon.Domain.Common;

namespace BeautySalon.Domain.Entities;

// One row per configured reminder lead time (15/30/60/1440 min), data-driven instead
// of a fixed enum so the user can enable/disable or add lead times later.
public class NotificationRule : AuditableEntity
{
    public Guid AppSettingsId { get; set; }
    public AppSettings? AppSettings { get; set; }

    public int MinutesBefore { get; set; }
    public bool IsEnabled { get; set; } = true;
}
