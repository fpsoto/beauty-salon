using System.Collections.ObjectModel;
using Beauty_Salon.Resources.Strings;
using Beauty_Salon.Services;
using BeautySalon.Application.Common;
using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Application.Features.Catalog;
using BeautySalon.Application.Features.Clients;
using BeautySalon.Application.Features.Schedule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class AppointmentFormViewModel : ViewModelBase
{
    private readonly IAppointmentAppService _appointmentAppService;
    private readonly IClientAppService _clientAppService;
    private readonly ICatalogAppService _catalogAppService;
    private readonly IAppointmentNotificationScheduler _notificationScheduler;
    private readonly ICurrentUserContext _currentUserContext;

    // Falls back to the seeded admin only as a defensive default - this page is only
    // reachable post-login, so _currentUserContext.UserId should always be set by then.
    private Guid ProfessionalId => _currentUserContext.UserId ?? WellKnownIds.AdminUserId;

    public AppointmentFormViewModel(
        IAppointmentAppService appointmentAppService,
        IClientAppService clientAppService,
        ICatalogAppService catalogAppService,
        IAppointmentNotificationScheduler notificationScheduler,
        ICurrentUserContext currentUserContext,
        ILogger<AppointmentFormViewModel> logger) : base(logger)
    {
        _appointmentAppService = appointmentAppService;
        _clientAppService = clientAppService;
        _catalogAppService = catalogAppService;
        _notificationScheduler = notificationScheduler;
        _currentUserContext = currentUserContext;
        AppointmentDate = DateTime.Today;
        AppointmentTime = new TimeSpan(9, 0, 0);
    }

    [ObservableProperty]
    private string clientSearchTerm = string.Empty;

    [ObservableProperty]
    private ClientDto? selectedClient;

    [ObservableProperty]
    private DateTime appointmentDate;

    [ObservableProperty]
    private TimeSpan appointmentTime;

    [ObservableProperty]
    private string? notes;

    [ObservableProperty]
    private bool created;

    public ObservableCollection<ClientDto> ClientResults { get; } = [];
    public ObservableCollection<SelectableService> Services { get; } = [];

    public decimal TotalPrice => Services.Where(s => s.IsSelected).Sum(s => s.Service.SuggestedPrice);
    public int TotalDurationMinutes => Services.Where(s => s.IsSelected).Sum(s => s.Service.DurationMinutes);

    public void RecomputeTotals()
    {
        OnPropertyChanged(nameof(TotalPrice));
        OnPropertyChanged(nameof(TotalDurationMinutes));
    }

    [RelayCommand]
    private Task LoadServicesAsync() => SafeExecuteAsync(async () =>
    {
        var result = await _catalogAppService.GetServicesAsync(null, true);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        Services.Clear();
        foreach (var service in result.Value)
            Services.Add(new SelectableService { Service = service });
    });

    [RelayCommand]
    private Task SearchClientsAsync() => SafeExecuteAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(ClientSearchTerm) || SelectedClient is not null)
        {
            ClientResults.Clear();
            return;
        }

        var result = await _clientAppService.SearchAsync(ClientSearchTerm);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        ClientResults.Clear();
        foreach (var client in result.Value)
            ClientResults.Add(client);
    });

    [RelayCommand]
    private void SelectClient(ClientDto client)
    {
        SelectedClient = client;
        ClientResults.Clear();
        ClientSearchTerm = $"{client.Name} {client.LastName}";
    }

    [RelayCommand]
    private void ClearSelectedClient()
    {
        SelectedClient = null;
        ClientSearchTerm = string.Empty;
    }

    [RelayCommand]
    private Task SaveAsync() => SafeExecuteAsync(async () =>
    {
        if (SelectedClient is null)
        {
            ErrorMessage = AppResources.SelectClientRequired;
            return;
        }

        var serviceIds = Services.Where(s => s.IsSelected).Select(s => s.Service.Id).ToList();
        if (serviceIds.Count == 0)
        {
            ErrorMessage = AppResources.SelectServiceRequired;
            return;
        }

        var request = new CreateAppointmentRequest(
            SelectedClient.Id,
            ProfessionalId,
            DateOnly.FromDateTime(AppointmentDate),
            TimeOnly.FromTimeSpan(AppointmentTime),
            serviceIds,
            Notes);

        var result = await _appointmentAppService.CreateAsync(request);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        await _notificationScheduler.ScheduleRemindersAsync(
            result.Value.Id, result.Value.ClientFullName, result.Value.Date, result.Value.StartTime);

        Created = true;
    });
}
