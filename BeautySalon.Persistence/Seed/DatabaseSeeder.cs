using BeautySalon.Application.Common;
using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Entities;
using BeautySalon.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Persistence.Seed;

// Populates first-run data: the admin user, a starter service catalog, payment
// methods, default settings/notification rules, and a Mon-Fri working schedule so
// the agenda isn't empty on first launch.
public class DatabaseSeeder
{
    private readonly BeautySalonDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public DatabaseSeeder(BeautySalonDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var admin = await SeedAdminUserAsync(cancellationToken);
        await SeedWorkingHoursAsync(admin.Id, cancellationToken);
        await SeedCatalogAsync(cancellationToken);
        await SeedPaymentMethodsAsync(cancellationToken);
        await SeedAppSettingsAsync(cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<User> SeedAdminUserAsync(CancellationToken cancellationToken)
    {
        var existing = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == WellKnownIds.AdminUserId, cancellationToken);
        if (existing is not null)
            return existing;

        var admin = new User
        {
            Id = WellKnownIds.AdminUserId,
            Username = "admin",
            PasswordHash = _passwordHasher.Hash("admin123"),
            FullName = "Administrador",
            Role = UserRole.Admin,
            IsActive = true
        };

        _context.Users.Add(admin);
        return admin;
    }

    private async Task SeedWorkingHoursAsync(Guid professionalId, CancellationToken cancellationToken)
    {
        if (await _context.WorkingHours.AnyAsync(w => w.ProfessionalId == professionalId, cancellationToken))
            return;

        var workDays = new[]
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday
        };

        foreach (var day in Enum.GetValues<DayOfWeek>())
        {
            _context.WorkingHours.Add(new WorkingHours
            {
                ProfessionalId = professionalId,
                DayOfWeek = day,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(18, 0),
                IsWorkingDay = workDays.Contains(day)
            });
        }
    }

    private async Task SeedCatalogAsync(CancellationToken cancellationToken)
    {
        if (await _context.ServiceCategories.AnyAsync(cancellationToken))
            return;

        var categories = new (string Name, string ColorHex, string[] Services)[]
        {
            ("Cabello", "#8E44AD", ["Corte Hombre", "Corte Mujer", "Balayage", "Tintura"]),
            ("Manicure", "#E91E63", ["Permanente", "Tradicional"]),
            ("Masajes", "#16A085", ["Relajación", "Descontracturante"])
        };

        // Starting durations/prices per the brief - editable later, only unblocks day-one usability.
        var defaults = new Dictionary<string, (int DurationMinutes, decimal Price)>
        {
            ["Corte Hombre"] = (30, 8_000m),
            ["Corte Mujer"] = (45, 12_000m),
            ["Balayage"] = (240, 60_000m),
            ["Tintura"] = (120, 35_000m),
            ["Permanente"] = (60, 15_000m),
            ["Tradicional"] = (45, 10_000m),
            ["Relajación"] = (60, 20_000m),
            ["Descontracturante"] = (60, 22_000m)
        };

        foreach (var (name, colorHex, services) in categories)
        {
            var category = new ServiceCategory { Name = name, ColorHex = colorHex, IsActive = true };

            foreach (var serviceName in services)
            {
                var (durationMinutes, price) = defaults[serviceName];
                category.Services.Add(new SalonService
                {
                    Name = serviceName,
                    DurationMinutes = durationMinutes,
                    SuggestedPrice = price,
                    IsActive = true
                });
            }

            _context.ServiceCategories.Add(category);
        }
    }

    private async Task SeedPaymentMethodsAsync(CancellationToken cancellationToken)
    {
        if (await _context.PaymentMethods.AnyAsync(cancellationToken))
            return;

        string[] paymentMethodNames = ["Efectivo", "Transferencia", "Débito", "Crédito", "Otro"];

        for (var i = 0; i < paymentMethodNames.Length; i++)
        {
            _context.PaymentMethods.Add(new PaymentMethod { Name = paymentMethodNames[i], IsActive = true, SortOrder = i });
        }
    }

    private async Task SeedAppSettingsAsync(CancellationToken cancellationToken)
    {
        if (await _context.AppSettings.AnyAsync(cancellationToken))
            return;

        var settings = new AppSettings
        {
            Language = "es",
            Theme = AppTheme.System,
            Currency = Currency.CLP,
            DateFormat = "dd/MM/yyyy",
            TimeFormat = "HH:mm"
        };

        foreach (var minutesBefore in new[] { 15, 30, 60, 1440 })
        {
            settings.NotificationRules.Add(new NotificationRule { MinutesBefore = minutesBefore, IsEnabled = true });
        }

        _context.AppSettings.Add(settings);
    }
}
