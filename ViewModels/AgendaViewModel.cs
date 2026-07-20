using System.Collections.ObjectModel;
using Beauty_Salon.Services;
using BeautySalon.Application.Common;
using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Application.Features.Schedule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

// The weekly agenda is the app's home screen (not a dashboard) - navigate weeks,
// jump to today, and drive the full appointment lifecycle from here.
public partial class AgendaViewModel : ViewModelBase
{
    private readonly IAppointmentAppService _appointmentAppService;
    private readonly IScheduleBlockAppService _scheduleBlockAppService;
    private readonly IAppointmentNotificationScheduler _notificationScheduler;
    private readonly ICurrentUserContext _currentUserContext;

    // Falls back to the seeded admin only as a defensive default - this page is only
    // reachable post-login, so _currentUserContext.UserId should always be set by then.
    private Guid ProfessionalId => _currentUserContext.UserId ?? WellKnownIds.AdminUserId;

    public AgendaViewModel(
        IAppointmentAppService appointmentAppService,
        IScheduleBlockAppService scheduleBlockAppService,
        IAppointmentNotificationScheduler notificationScheduler,
        ICurrentUserContext currentUserContext,
        ILogger<AgendaViewModel> logger) : base(logger)
    {
        _appointmentAppService = appointmentAppService;
        _scheduleBlockAppService = scheduleBlockAppService;
        _notificationScheduler = notificationScheduler;
        _currentUserContext = currentUserContext;
        SelectedDate = DateOnly.FromDateTime(DateTime.Now);
    }

    [ObservableProperty]
    private DateOnly selectedDate;

    [ObservableProperty]
    private DateOnly weekStart;

    [ObservableProperty]
    private DateOnly weekEnd;

    public ObservableCollection<AppointmentDto> Appointments { get; } = [];
    public ObservableCollection<ScheduleBlockDto> Blocks { get; } = [];
    public ObservableCollection<DayAgendaGroup> DayGroups { get; } = [];

    [RelayCommand]
    private Task LoadWeekAsync() => SafeExecuteAsync(LoadWeekCoreAsync);

    [RelayCommand]
    private Task GoToPreviousWeekAsync() => SafeExecuteAsync(async () =>
    {
        SelectedDate = SelectedDate.AddDays(-7);
        await LoadWeekCoreAsync();
    });

    [RelayCommand]
    private Task GoToNextWeekAsync() => SafeExecuteAsync(async () =>
    {
        SelectedDate = SelectedDate.AddDays(7);
        await LoadWeekCoreAsync();
    });

    [RelayCommand]
    private Task GoToTodayAsync() => SafeExecuteAsync(async () =>
    {
        SelectedDate = DateOnly.FromDateTime(DateTime.Now);
        await LoadWeekCoreAsync();
    });

    public Task CreateAppointmentAsync(CreateAppointmentRequest request) => SafeExecuteAsync(async () =>
    {
        var result = await _appointmentAppService.CreateAsync(request);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        await LoadWeekCoreAsync();
    });

    public Task RescheduleAppointmentAsync(RescheduleAppointmentRequest request) => SafeExecuteAsync(async () =>
    {
        var result = await _appointmentAppService.RescheduleAsync(request);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        await LoadWeekCoreAsync();
    });

    public Task ConfirmAppointmentAsync(Guid appointmentId) => SafeExecuteAsync(async () =>
    {
        var result = await _appointmentAppService.ConfirmAsync(appointmentId);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        await LoadWeekCoreAsync();
    });

    public Task StartAppointmentAsync(Guid appointmentId) => SafeExecuteAsync(async () =>
    {
        var result = await _appointmentAppService.StartAsync(appointmentId);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        await LoadWeekCoreAsync();
    });

    public Task CancelAppointmentAsync(Guid appointmentId, string? reason = null) => SafeExecuteAsync(async () =>
    {
        var result = await _appointmentAppService.CancelAsync(appointmentId, reason);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        await _notificationScheduler.CancelRemindersAsync(appointmentId);
        await LoadWeekCoreAsync();
    });

    public Task MarkNoShowAsync(Guid appointmentId) => SafeExecuteAsync(async () =>
    {
        var result = await _appointmentAppService.MarkNoShowAsync(appointmentId);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        await _notificationScheduler.CancelRemindersAsync(appointmentId);
        await LoadWeekCoreAsync();
    });

    public Task FinishAppointmentAsync(FinishAppointmentRequest request) => SafeExecuteAsync(async () =>
    {
        var result = await _appointmentAppService.FinishAsync(request);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        await LoadWeekCoreAsync();
    });

    public Task CreateBlockAsync(CreateScheduleBlockRequest request) => SafeExecuteAsync(async () =>
    {
        var result = await _scheduleBlockAppService.CreateAsync(request);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        await LoadWeekCoreAsync();
    });

    public Task DeleteBlockAsync(Guid scheduleBlockId) => SafeExecuteAsync(async () =>
    {
        var result = await _scheduleBlockAppService.DeleteAsync(scheduleBlockId);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        await LoadWeekCoreAsync();
    });

    private async Task LoadWeekCoreAsync()
    {
        var result = await _appointmentAppService.GetWeekAsync(SelectedDate, ProfessionalId);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        WeekStart = result.Value.WeekStart;
        WeekEnd = result.Value.WeekEnd;

        Appointments.Clear();
        foreach (var appointment in result.Value.Appointments)
            Appointments.Add(appointment);

        Blocks.Clear();
        foreach (var block in result.Value.Blocks)
            Blocks.Add(block);

        RebuildDayGroups();
    }

    private void RebuildDayGroups()
    {
        var entries = Appointments.Select(AgendaEntry.FromAppointment)
            .Concat(Blocks.Select(AgendaEntry.FromBlock));

        var byDay = entries
            .GroupBy(e => e.Appointment?.Date ?? e.Block!.Date)
            .ToDictionary(g => g.Key, g => g.OrderBy(e => e.StartTime).ToList());

        DayGroups.Clear();
        for (var date = WeekStart; date <= WeekEnd; date = date.AddDays(1))
        {
            DayGroups.Add(new DayAgendaGroup(date, byDay.TryGetValue(date, out var dayEntries) ? dayEntries : []));
        }
    }
}
