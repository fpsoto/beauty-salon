namespace Beauty_Salon.Services;

// Wraps Plugin.LocalNotification so ViewModels never depend on it directly - schedules one
// local reminder per enabled NotificationRule (Settings) ahead of an appointment's start time.
public interface IAppointmentNotificationScheduler
{
    Task ScheduleRemindersAsync(Guid appointmentId, string clientFullName, DateOnly date, TimeOnly startTime);
    Task CancelRemindersAsync(Guid appointmentId);
}
