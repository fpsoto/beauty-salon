using BeautySalon.Domain.Common;
using BeautySalon.Domain.Enums;

namespace BeautySalon.Domain.Entities;

// Single-row settings entity. Lives in the database (not device Preferences) so it
// is included in the future automatic backup of the .db3 file.
public class AppSettings : AuditableEntity
{
    public string Language { get; set; } = "es";
    public AppTheme Theme { get; set; } = AppTheme.System;
    public Currency Currency { get; set; } = Currency.CLP;
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string TimeFormat { get; set; } = "HH:mm";

    public ICollection<NotificationRule> NotificationRules { get; set; } = [];
}
