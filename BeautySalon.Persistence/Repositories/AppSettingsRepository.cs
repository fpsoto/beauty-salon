using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Persistence.Repositories;

// Single-row aggregate - no generic EfRepository<T> base needed, just read/persist the one row.
public class AppSettingsRepository : IAppSettingsRepository
{
    private readonly BeautySalonDbContext _context;

    public AppSettingsRepository(BeautySalonDbContext context)
    {
        _context = context;
    }

    public async Task<AppSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _context.AppSettings
            .Include(s => s.NotificationRules)
            .FirstOrDefaultAsync(cancellationToken);

        return settings ?? throw new InvalidOperationException("AppSettings has not been seeded yet.");
    }

    public void Update(AppSettings appSettings) => _context.AppSettings.Update(appSettings);
}
