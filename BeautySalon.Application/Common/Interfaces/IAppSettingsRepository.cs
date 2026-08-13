using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Common.Interfaces;

// Single-row settings aggregate - no generic CRUD needed, just read/persist the one row.
public interface IAppSettingsRepository
{
    Task<AppSettings> GetAsync(CancellationToken cancellationToken = default);
    void Update(AppSettings appSettings);

    // Explicit Add rather than relying on settings.NotificationRules.Add(rule) + automatic
    // change detection: NotificationRule.Id is a non-default client-generated Guid (BaseEntity),
    // so EF's automatic graph discovery for a newly-added child of an already-tracked parent
    // would otherwise guess it's an existing row to update rather than a new one to insert.
    void AddNotificationRule(NotificationRule rule);
}
