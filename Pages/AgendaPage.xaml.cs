using Beauty_Salon.ViewModels;
using BeautySalon.Domain.Enums;

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
    }

    private async void OnEntrySelected(object? sender, SelectionChangedEventArgs e)
    {
        AgendaCollectionView.SelectedItem = null;

        if (e.CurrentSelection.FirstOrDefault() is not AgendaEntry { IsBlock: false, Appointment: { } appointment })
            return;

        var actions = BuildAvailableActions(appointment.Status);
        if (actions.Count == 0)
            return;

        var choice = await DisplayActionSheetAsync($"{appointment.ClientFullName} - {appointment.StartTime:HH:mm}", "Cerrar", null, [.. actions]);

        switch (choice)
        {
            case "Confirmar":
                await _viewModel.ConfirmAppointmentAsync(appointment.Id);
                break;
            case "Iniciar":
                await _viewModel.StartAppointmentAsync(appointment.Id);
                break;
            case "Finalizar":
                await Shell.Current.GoToAsync("appointment/finish", new Dictionary<string, object> { ["AppointmentId"] = appointment.Id });
                break;
            case "Reagendar":
                await Shell.Current.GoToAsync("appointment/reschedule", new Dictionary<string, object> { ["AppointmentId"] = appointment.Id });
                break;
            case "No asistió":
                await _viewModel.MarkNoShowAsync(appointment.Id);
                break;
            case "Cancelar cita":
                var reason = await DisplayPromptAsync("Cancelar cita", "Motivo (opcional):");
                await _viewModel.CancelAppointmentAsync(appointment.Id, reason);
                break;
        }
    }

    private static List<string> BuildAvailableActions(AppointmentStatus status)
    {
        var actions = new List<string>();

        if (status == AppointmentStatus.Booked)
            actions.Add("Confirmar");

        if (status is AppointmentStatus.Booked or AppointmentStatus.Confirmed)
            actions.Add("Iniciar");

        if (status is AppointmentStatus.Booked or AppointmentStatus.Confirmed or AppointmentStatus.InProgress)
            actions.Add("Finalizar");

        if (status is AppointmentStatus.Booked or AppointmentStatus.Confirmed)
        {
            actions.Add("Reagendar");
            actions.Add("No asistió");
        }

        if (status is AppointmentStatus.Booked or AppointmentStatus.Confirmed or AppointmentStatus.InProgress)
            actions.Add("Cancelar cita");

        return actions;
    }

    private async void OnCreateAppointmentClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("appointment/new");
}
