using System.Collections.ObjectModel;
using BeautySalon.Application.Common;
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

    // TODO(Fase 5+): replace with the signed-in professional's id once login/session wiring supports it.
    private readonly Guid _professionalId = WellKnownIds.AdminUserId;

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

    public SettingsViewModel(ISettingsAppService settingsAppService, ILogger<SettingsViewModel> logger) : base(logger)
    {
        _settingsAppService = settingsAppService;
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

        var hoursResult = await _settingsAppService.GetWorkingHoursAsync(_professionalId);
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
        var workingHoursResult = await _settingsAppService.UpdateWorkingHoursAsync(_professionalId, new UpdateWorkingHoursRequest(workingHoursInputs));
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
}
