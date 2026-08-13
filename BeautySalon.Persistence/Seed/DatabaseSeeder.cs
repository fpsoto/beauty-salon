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

    // Placeholder price for every seeded service - the real catalog (given by the user) has no
    // pricing yet, so every service starts at this flat value and gets edited later from Catalog.
    private const decimal PlaceholderPrice = 15_000m;

    // Default duration for services with no explicit timing in the source catalog; the three
    // "Masaje Mixto" services DO carry an explicit duration in their own name (40/60/80 min).
    private const int DefaultDurationMinutes = 60;

    private async Task SeedCatalogAsync(CancellationToken cancellationToken)
    {
        if (await _context.ServiceCategories.AnyAsync(cancellationToken))
            return;

        // Real catalog provided by the user. The source list has a two-level structure
        // (e.g. Estética > Pestañas > Lifting de pestañas) but ServiceCategory/SalonService is
        // flat, so each subsection name is folded into its services' names ("Pestañas: ...").
        var categories = new (string Name, string ColorHex, (string Service, int? DurationMinutes)[] Services)[]
        {
            ("Estética", "#8E44AD",
            [
                ("Pestañas: Lifting de pestañas (Protocolo Coreano)", null),
                ("Pestañas: Lifting de pestañas Tradicional", null),
                ("Pestañas: Tratamiento Nutritivo para Pestañas", null),
                ("Cejas: Brow Lamination + Perfilado", null),
                ("Cejas: Perfilado + Henna", null),
                ("Cejas: Brow Lamination + Perfilado + Henna", null),
                ("Faciales: Spa Facial", null),
                ("Faciales: Limpieza Facial Básica", null),
                ("Faciales: Limpieza Facial Profunda", null),
                ("Espalda: Spa y Limpieza de Espalda", null),
                ("Espalda: Spa y Limpieza Profunda de Espalda", null)
            ]),
            ("Depilación con Cera", "#E91E63",
            [
                ("Rostro: Cejas", null),
                ("Rostro: Bozo", null),
                ("Rostro: Mentón", null),
                ("Rostro: Rostro Completo", null),
                ("Cuerpo: Axilas", null),
                ("Cuerpo: Brazos Completos", null),
                ("Cuerpo: Media Pierna", null),
                ("Cuerpo: Piernas Completas", null),
                ("Zona Íntima: Rebaje Corto (Simple)", null),
                ("Zona Íntima: Rebaje Completo", null)
            ]),
            ("Depilación Semidefinitiva", "#F39C12",
            [
                ("Zona Cuerpo", null),
                ("Zona Íntima", null)
            ]),
            ("Masajes", "#16A085",
            [
                ("Masaje Mixto: Express", 40),
                ("Masaje Mixto: Normal", 60),
                ("Masaje Mixto: Premium", 80)
            ])
        };

        foreach (var (name, colorHex, services) in categories)
        {
            var category = new ServiceCategory { Name = name, ColorHex = colorHex, IsActive = true };

            foreach (var (serviceName, durationMinutes) in services)
            {
                category.Services.Add(new SalonService
                {
                    Name = serviceName,
                    DurationMinutes = durationMinutes ?? DefaultDurationMinutes,
                    SuggestedPrice = PlaceholderPrice,
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
