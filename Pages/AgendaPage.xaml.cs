using Beauty_Salon.Resources.Strings;
using Beauty_Salon.ViewModels;
using BeautySalon.Application.Features.Schedule;
using BeautySalon.Domain.Enums;
using Plugin.LocalNotification;

namespace Beauty_Salon.Pages;

public partial class AgendaPage : ContentPage
{
    private readonly AgendaViewModel _viewModel;

    public AgendaPage(AgendaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.LoadWeekCommand.ExecuteAsync(null);
        _ = RequestNotificationPermissionAsync();
    }

    private static async Task RequestNotificationPermissionAsync()
    {
        if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
            await LocalNotificationCenter.Current.RequestNotificationPermission();
    }

    private async void OnEntrySelected(object? sender, SelectionChangedEventArgs e)
    {
        AgendaCollectionView.SelectedItem = null;

        if (e.CurrentSelection.FirstOrDefault() is not AgendaEntry { IsBlock: false, Appointment: { } appointment })
            return;

        var actions = BuildAvailableActions(appointment.Status, appointment.ClientPhone);
        if (actions.Count == 0)
            return;

        var choice = await DisplayActionSheetAsync(
            $"{appointment.ClientFullName} - {appointment.StartTime:HH:mm}", AppResources.Close, null, [.. actions]);

        if (choice == AppResources.ConfirmAction)
            await _viewModel.ConfirmAppointmentAsync(appointment.Id);
        else if (choice == AppResources.StartAction)
            await _viewModel.StartAppointmentAsync(appointment.Id);
        else if (choice == AppResources.FinishAction)
            await Shell.Current.GoToAsync("appointment/finish", new Dictionary<string, object> { ["AppointmentId"] = appointment.Id });
        else if (choice == AppResources.RescheduleAction)
            await Shell.Current.GoToAsync("appointment/reschedule", new Dictionary<string, object> { ["AppointmentId"] = appointment.Id });
        else if (choice == AppResources.NoShowAction)
            await _viewModel.MarkNoShowAsync(appointment.Id);
        else if (choice == AppResources.CancelAppointmentAction)
        {
            var reason = await DisplayPromptAsync(AppResources.CancelAppointmentAction, AppResources.CancelReasonPrompt);
            await _viewModel.CancelAppointmentAsync(appointment.Id, reason);
        }
        else if (choice == AppResources.SendReminderAction)
            await SendWhatsAppReminderAsync(appointment);
    }

    private static async Task SendWhatsAppReminderAsync(AppointmentDto appointment)
    {
        // wa.me expects digits only (country code + number, no "+"/spaces/dashes).
        var digitsOnly = new string(appointment.ClientPhone.Where(char.IsDigit).ToArray());
        if (digitsOnly.Length == 0)
            return;

        var message = string.Format(
            AppResources.WhatsAppReminderMessageFormat,
            appointment.ClientFullName,
            appointment.Date.ToDateTime(TimeOnly.MinValue),
            appointment.Date.ToDateTime(appointment.StartTime));

        var uri = new Uri($"https://wa.me/{digitsOnly}?text={Uri.EscapeDataString(message)}");
        await Launcher.Default.OpenAsync(uri);
    }

    private static List<string> BuildAvailableActions(AppointmentStatus status, string clientPhone)
    {
        var actions = new List<string>();

        if (status == AppointmentStatus.Booked)
            actions.Add(AppResources.ConfirmAction);

        if (status is AppointmentStatus.Booked or AppointmentStatus.Confirmed)
            actions.Add(AppResources.StartAction);

        if (status is AppointmentStatus.Booked or AppointmentStatus.Confirmed or AppointmentStatus.InProgress)
            actions.Add(AppResources.FinishAction);

        if (status is AppointmentStatus.Booked or AppointmentStatus.Confirmed)
        {
            actions.Add(AppResources.RescheduleAction);
            actions.Add(AppResources.NoShowAction);

            if (!string.IsNullOrWhiteSpace(clientPhone))
                actions.Add(AppResources.SendReminderAction);
        }

        if (status is AppointmentStatus.Booked or AppointmentStatus.Confirmed or AppointmentStatus.InProgress)
            actions.Add(AppResources.CancelAppointmentAction);

        return actions;
    }

    private async void OnCreateAppointmentClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("appointment/new");
}
