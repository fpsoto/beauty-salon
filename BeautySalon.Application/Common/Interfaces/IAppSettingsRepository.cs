using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Common.Interfaces;

// Single-row settings aggregate - no generic CRUD needed, just read/persist the one row.
public interface IAppSettingsRepository
{
    Task<AppSettings> GetAsync(CancellationToken cancellationToken = default);
    void Update(AppSettings appSettings);
}
