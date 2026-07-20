using System.Collections.ObjectModel;
using BeautySalon.Application.Common;
using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Application.Features.Catalog;
using BeautySalon.Application.Features.Clients;
using BeautySalon.Application.Features.Payments;
using BeautySalon.Application.Features.Schedule;
using BeautySalon.Application.Features.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
// Disambiguates from Microsoft.Maui.ApplicationModel.AppTheme (system light/dark detection).
using AppTheme = BeautySalon.Domain.Enums.AppTheme;
using Currency = BeautySalon.Domain.Enums.Currency;

namespace Beauty_Salon.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsAppService _settingsAppService;
    private readonly ICurrentUserContext _currentUserContext;
    // TEMPORARY (remove along with SeedTestDataAsync/SeedResultMessage once test data has been seeded):
    private readonly IClientAppService _clientAppService;
    private readonly ICatalogAppService _catalogAppService;
    private readonly IAppointmentAppService _appointmentAppService;
    private readonly IPaymentMethodAppService _paymentMethodAppService;

    // Falls back to the seeded admin only as a defensive default - this page is only
    // reachable post-login, so _currentUserContext.UserId should always be set by then.
    private Guid ProfessionalId => _currentUserContext.UserId ?? WellKnownIds.AdminUserId;

    private static readonly (DayOfWeek Day, string Label)[] DayOrder =
    [
        (DayOfWeek.Monday, "Lunes"),
        (DayOfWeek.Tuesday, "Martes"),
        (DayOfWeek.Wednesday, "Miércoles"),
        (DayOfWeek.Thursday, "Jueves"),
        (DayOfWeek.Friday, "Viernes"),
        (DayOfWeek.Saturday, "Sábado"),
        (DayOfWeek.Sunday, "Domingo")
    ];

    public SettingsViewModel(
        ISettingsAppService settingsAppService,
        ICurrentUserContext currentUserContext,
        IClientAppService clientAppService,
        ICatalogAppService catalogAppService,
        IAppointmentAppService appointmentAppService,
        IPaymentMethodAppService paymentMethodAppService,
        ILogger<SettingsViewModel> logger) : base(logger)
    {
        _settingsAppService = settingsAppService;
        _currentUserContext = currentUserContext;
        _clientAppService = clientAppService;
        _catalogAppService = catalogAppService;
        _appointmentAppService = appointmentAppService;
        _paymentMethodAppService = paymentMethodAppService;
    }

    [ObservableProperty]
    private string language = "es";

    [ObservableProperty]
    private AppTheme theme = AppTheme.System;

    [ObservableProperty]
    private Currency currency = Currency.CLP;

    [ObservableProperty]
    private string dateFormat = "dd/MM/yyyy";

    [ObservableProperty]
    private string timeFormat = "HH:mm";

    [ObservableProperty]
    private bool saved;

    // TEMPORARY: seeding status shown next to the debug seed button - remove with SeedTestDataAsync.
    [ObservableProperty]
    private string? seedResultMessage;

    public ObservableCollection<WorkingDayEditItem> WorkingDays { get; } = [];
    public ObservableCollection<NotificationRuleEditItem> NotificationRules { get; } = [];

    [RelayCommand]
    private Task LoadAsync() => SafeExecuteAsync(async () =>
    {
        var settingsResult = await _settingsAppService.GetSettingsAsync();
        if (settingsResult.IsFailure)
        {
            SetError(settingsResult.Error);
            return;
        }

        Language = settingsResult.Value.Language;
        Theme = settingsResult.Value.Theme;
        Currency = settingsResult.Value.Currency;
        DateFormat = settingsResult.Value.DateFormat;
        TimeFormat = settingsResult.Value.TimeFormat;

        NotificationRules.Clear();
        foreach (var minutesBefore in new[] { 15, 30, 60, 1440 })
        {
            var existing = settingsResult.Value.NotificationRules.FirstOrDefault(r => r.MinutesBefore == minutesBefore);
            NotificationRules.Add(new NotificationRuleEditItem
            {
                MinutesBefore = minutesBefore,
                Label = FormatLeadTime(minutesBefore),
                IsEnabled = existing?.IsEnabled ?? false
            });
        }

        var hoursResult = await _settingsAppService.GetWorkingHoursAsync(ProfessionalId);
        if (hoursResult.IsFailure)
        {
            SetError(hoursResult.Error);
            return;
        }

        WorkingDays.Clear();
        foreach (var (day, label) in DayOrder)
        {
            var existing = hoursResult.Value.FirstOrDefault(h => h.DayOfWeek == day);
            WorkingDays.Add(new WorkingDayEditItem
            {
                DayOfWeek = day,
                DayLabel = label,
                IsWorkingDay = existing?.IsWorkingDay ?? false,
                StartTime = (existing?.StartTime ?? new TimeOnly(9, 0)).ToTimeSpan(),
                EndTime = (existing?.EndTime ?? new TimeOnly(18, 0)).ToTimeSpan()
            });
        }
    });

    [RelayCommand]
    private Task SaveAsync() => SafeExecuteAsync(async () =>
    {
        var updateSettingsResult = await _settingsAppService.UpdateSettingsAsync(
            new UpdateAppSettingsRequest(Language, Theme, Currency, DateFormat, TimeFormat));
        if (updateSettingsResult.IsFailure)
        {
            SetError(updateSettingsResult.Error);
            return;
        }

        var notificationInputs = NotificationRules.Select(r => new NotificationRuleInput(r.MinutesBefore, r.IsEnabled)).ToList();
        var notificationResult = await _settingsAppService.UpdateNotificationRulesAsync(notificationInputs);
        if (notificationResult.IsFailure)
        {
            SetError(notificationResult.Error);
            return;
        }

        var workingHoursInputs = WorkingDays
            .Select(d => new WorkingHoursInput(d.DayOfWeek, TimeOnly.FromTimeSpan(d.StartTime), TimeOnly.FromTimeSpan(d.EndTime), d.IsWorkingDay))
            .ToList();
        var workingHoursResult = await _settingsAppService.UpdateWorkingHoursAsync(ProfessionalId, new UpdateWorkingHoursRequest(workingHoursInputs));
        if (workingHoursResult.IsFailure)
        {
            SetError(workingHoursResult.Error);
            return;
        }

        Saved = true;
    });

    private static string FormatLeadTime(int minutesBefore) => minutesBefore switch
    {
        15 => "15 minutos antes",
        30 => "30 minutos antes",
        60 => "1 hora antes",
        1440 => "1 día antes",
        _ => $"{minutesBefore} minutos antes"
    };

    // TEMPORARY: one-off debug seeder to generate random clients/appointments for manually
    // testing Reports. Remove this command, SeedResultMessage, the extra constructor
    // dependencies above, and the button in SettingsPage.xaml once no longer needed.
    [RelayCommand]
    private Task SeedTestDataAsync() => SafeExecuteAsync(async () =>
    {
        SeedResultMessage = "Sembrando...";

        var rnd = new Random();
        string[] firstNames = ["María", "Camila", "Javiera", "Fernanda", "Valentina", "Constanza", "Antonia",
            "Francisca", "Josefa", "Catalina", "Trinidad", "Isidora", "Florencia", "Martina", "Amanda", "Sofía",
            "Emilia", "Renata"];
        string[] lastNames = ["González", "Muñoz", "Rojas", "Díaz", "Pérez", "Soto", "Contreras", "Silva",
            "Martínez", "Sepúlveda", "Morales", "Rodríguez", "López", "Fuentes", "Hernández", "Torres", "Araya",
            "Flores"];

        static char ComputeCheckDigit(string body)
        {
            var sum = 0;
            var factor = 2;
            for (var i = body.Length - 1; i >= 0; i--)
            {
                sum += (body[i] - '0') * factor;
                factor = factor == 7 ? 2 : factor + 1;
            }
            var remainder = 11 - (sum % 11);
            return remainder switch { 11 => '0', 10 => 'K', _ => (char)('0' + remainder) };
        }

        string RandomRut()
        {
            var body = rnd.Next(5_000_000, 25_000_000).ToString();
            return $"{body}-{ComputeCheckDigit(body)}";
        }

        var clientIds = new List<Guid>();
        var usedRuts = new HashSet<string>();
        for (var i = 0; i < 18; i++)
        {
            string rut;
            do { rut = RandomRut(); } while (!usedRuts.Add(rut));

            var name = firstNames[rnd.Next(firstNames.Length)];
            var lastName = lastNames[rnd.Next(lastNames.Length)];
            var phone = $"+56 9 {rnd.Next(10_000_000, 99_999_999)}";
            var email = rnd.NextDouble() < 0.7 ? $"{name.ToLowerInvariant()}.{lastName.ToLowerInvariant()}{i}@example.com" : null;
            var dob = rnd.NextDouble() < 0.5
                ? DateOnly.FromDateTime(DateTime.Today.AddYears(-rnd.Next(18, 60)).AddDays(-rnd.Next(365)))
                : (DateOnly?)null;

            var clientResult = await _clientAppService.CreateAsync(new CreateClientRequest(name, lastName, rut, phone, email, dob, null, null));
            if (clientResult.IsSuccess)
                clientIds.Add(clientResult.Value.Id);
        }

        var servicesResult = await _catalogAppService.GetServicesAsync(null, true);
        var catalogServices = servicesResult.IsSuccess ? servicesResult.Value : [];

        var paymentMethodsResult = await _paymentMethodAppService.GetAllAsync(true);
        var paymentMethods = paymentMethodsResult.IsSuccess ? paymentMethodsResult.Value : [];

        if (clientIds.Count == 0 || catalogServices.Count == 0 || paymentMethods.Count == 0)
        {
            SeedResultMessage = "Faltan clientes/servicios/métodos de pago - no se crearon citas.";
            return;
        }

        TimeOnly[] slots = [new(9, 0), new(11, 30), new(14, 0), new(16, 0)];
        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = today.AddDays(-70);
        var end = today.AddDays(7);

        var created = 0;
        var completed = 0;
        var cancelled = 0;
        var noShow = 0;

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                continue;

            if (rnd.NextDouble() < 0.3)
                continue;

            var slotCount = rnd.Next(1, 3);
            for (var s = 0; s < slotCount && s < slots.Length; s++)
            {
                var clientId = clientIds[rnd.Next(clientIds.Count)];
                var service = catalogServices[rnd.Next(catalogServices.Count)];

                var createResult = await _appointmentAppService.CreateAsync(new CreateAppointmentRequest(
                    clientId, ProfessionalId, date, slots[s], [service.Id], null));
                if (createResult.IsFailure)
                    continue;

                created++;
                var appointmentId = createResult.Value.Id;
                var isPast = date < today;
                var roll = rnd.NextDouble();

                if (isPast)
                {
                    if (roll < 0.60)
                    {
                        await _appointmentAppService.ConfirmAsync(appointmentId);
                        await _appointmentAppService.StartAsync(appointmentId);
                        var discount = rnd.NextDouble() < 0.3 ? Math.Round(service.SuggestedPrice * 0.1m, 0) : 0m;
                        var tip = rnd.NextDouble() < 0.4 ? new[] { 1000m, 2000m, 5000m }[rnd.Next(3)] : 0m;
                        var paymentMethod = paymentMethods[rnd.Next(paymentMethods.Count)];
                        var finishResult = await _appointmentAppService.FinishAsync(new FinishAppointmentRequest(
                            appointmentId, service.SuggestedPrice - discount, discount, tip, paymentMethod.Id, null));
                        if (finishResult.IsSuccess)
                            completed++;
                    }
                    else if (roll < 0.75)
                    {
                        var reason = rnd.NextDouble() < 0.5 ? "Cliente reprogramó por su cuenta" : null;
                        if ((await _appointmentAppService.CancelAsync(appointmentId, reason)).IsSuccess)
                            cancelled++;
                    }
                    else if (roll < 0.85)
                    {
                        await _appointmentAppService.ConfirmAsync(appointmentId);
                        if ((await _appointmentAppService.MarkNoShowAsync(appointmentId)).IsSuccess)
                            noShow++;
                    }
                    else
                    {
                        await _appointmentAppService.ConfirmAsync(appointmentId);
                    }
                }
                else if (rnd.NextDouble() < 0.5)
                {
                    await _appointmentAppService.ConfirmAsync(appointmentId);
                }
            }
        }

        SeedResultMessage = $"Listo: {clientIds.Count} clientes, {created} citas ({completed} completadas, {cancelled} canceladas, {noShow} no-show).";
    });
}
