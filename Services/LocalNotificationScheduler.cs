using Beauty_Salon.Resources.Strings;
using BeautySalon.Application.Features.Settings;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using Plugin.LocalNotification.Core.Models.AndroidOption;

namespace Beauty_Salon.Services;

public sealed class LocalNotificationScheduler : IAppointmentNotificationScheduler
{
    public const string AppointmentReminderChannelId = "appointment_reminders";

    // Headroom above the 4 rules seeded by default (15/30/60/1440 min) - each enabled
    // rule gets its own reminder slot, so an appointment can have up to this many.
    private const int MaxRulesPerAppointment = 8;

    private readonly ISettingsAppService _settingsAppService;

    public LocalNotificationScheduler(ISettingsAppService settingsAppService)
    {
        _settingsAppService = settingsAppService;
    }

    public async Task ScheduleRemindersAsync(Guid appointmentId, string clientFullName, DateOnly date, TimeOnly startTime)
    {
        await CancelRemindersAsync(appointmentId);

        var settingsResult = await _settingsAppService.GetSettingsAsync();
        if (!settingsResult.IsSuccess)
            return;

        var appointmentDateTime = date.ToDateTime(startTime);
        var enabledRules = settingsResult.Value.NotificationRules
            .Where(r => r.IsEnabled)
            .Take(MaxRulesPerAppointment)
            .ToList();

        for (var slot = 0; slot < enabledRules.Count; slot++)
        {
            var notifyTime = appointmentDateTime.AddMinutes(-enabledRules[slot].MinutesBefore);
            if (notifyTime <= DateTime.Now)
                continue;

            var request = new NotificationRequest
            {
                NotificationId = GetNotificationId(appointmentId, slot),
                Title = AppResources.AppointmentReminderTitle,
                Description = string.Format(AppResources.AppointmentReminderMessageFormat, clientFullName, appointmentDateTime),
                Schedule = { NotifyTime = notifyTime },
                Android = { ChannelId = AppointmentReminderChannelId, IconSmallName = new AndroidIcon("ic_notification") }
            };

            await LocalNotificationCenter.Current.Show(request);
        }
    }

    public Task CancelRemindersAsync(Guid appointmentId)
    {
        var ids = Enumerable.Range(0, MaxRulesPerAppointment)
            .Select(slot => GetNotificationId(appointmentId, slot))
            .ToArray();

        LocalNotificationCenter.Current.Cancel(ids);
        return Task.CompletedTask;
    }

    // Deterministic id from the appointment's Guid + slot index, so the same appointment/rule
    // pair always resolves to the same notification id and can be cancelled without a lookup table.
    private static int GetNotificationId(Guid appointmentId, int slot)
    {
        var bytes = appointmentId.ToByteArray();
        var basis = BitConverter.ToInt32(bytes, 0) & 0x1FFFFFFF; // keep positive, leave 3 bits for the slot
        return (basis << 3) | (slot & 0x7);
    }
}
