using Beauty_Salon.Services;
using BeautySalon.Application.Features.Schedule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class RescheduleViewModel : ViewModelBase
{
    private readonly IAppointmentAppService _appointmentAppService;
    private readonly IAppointmentNotificationScheduler _notificationScheduler;
    private Guid _appointmentId;

    public RescheduleViewModel(
        IAppointmentAppService appointmentAppService,
        IAppointmentNotificationScheduler notificationScheduler,
        ILogger<RescheduleViewModel> logger) : base(logger)
    {
        _appointmentAppService = appointmentAppService;
        _notificationScheduler = notificationScheduler;
        NewDate = DateTime.Today;
        NewTime = new TimeSpan(9, 0, 0);
    }

    [ObservableProperty]
    private DateTime newDate;

    [ObservableProperty]
    private TimeSpan newTime;

    [ObservableProperty]
    private bool rescheduled;

    public void Initialize(Guid appointmentId) => _appointmentId = appointmentId;

    [RelayCommand]
    private Task SaveAsync() => SafeExecuteAsync(async () =>
    {
        var request = new RescheduleAppointmentRequest(_appointmentId, DateOnly.FromDateTime(NewDate), TimeOnly.FromTimeSpan(NewTime));
        var result = await _appointmentAppService.RescheduleAsync(request);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        await _notificationScheduler.CancelRemindersAsync(_appointmentId);
        await _notificationScheduler.ScheduleRemindersAsync(
            result.Value.Id, result.Value.ClientFullName, result.Value.Date, result.Value.StartTime);

        Rescheduled = true;
    });
}
