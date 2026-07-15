using BeautySalon.Application.Features.Clients;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class ClientFormViewModel : ViewModelBase
{
    private readonly IClientAppService _clientAppService;
    private Guid? _clientId;

    public ClientFormViewModel(IClientAppService clientAppService, ILogger<ClientFormViewModel> logger) : base(logger)
    {
        _clientAppService = clientAppService;
    }

    [ObservableProperty]
    private string title = "Nuevo cliente";

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string lastName = string.Empty;

    [ObservableProperty]
    private string rut = string.Empty;

    [ObservableProperty]
    private string phone = string.Empty;

    [ObservableProperty]
    private string? email;

    [ObservableProperty]
    private bool hasDateOfBirth;

    [ObservableProperty]
    private DateTime dateOfBirth = new(1990, 1, 1);

    [ObservableProperty]
    private string? address;

    [ObservableProperty]
    private string? notes;

    [ObservableProperty]
    private bool saved;

    public void Initialize(Guid? clientId)
    {
        _clientId = clientId;

        if (clientId is { } id && id != Guid.Empty)
        {
            Title = "Editar cliente";
            _ = LoadAsync(id);
        }
    }

    private Task LoadAsync(Guid clientId) => SafeExecuteAsync(async () =>
    {
        var result = await _clientAppService.GetDetailAsync(clientId);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        var client = result.Value.Client;
        Name = client.Name;
        LastName = client.LastName;
        Rut = client.Rut;
        Phone = client.Phone;
        Email = client.Email;
        Address = client.Address;
        Notes = client.Notes;

        if (client.DateOfBirth is { } dateOfBirthValue)
        {
            HasDateOfBirth = true;
            DateOfBirth = dateOfBirthValue.ToDateTime(TimeOnly.MinValue);
        }
    });

    [RelayCommand]
    private Task SaveAsync() => SafeExecuteAsync(async () =>
    {
        var dateOfBirthOnly = HasDateOfBirth ? DateOnly.FromDateTime(DateOfBirth) : (DateOnly?)null;

        if (_clientId is { } id && id != Guid.Empty)
        {
            var updateRequest = new UpdateClientRequest(Name, LastName, Rut, Phone, Email, dateOfBirthOnly, Address, Notes);
            var updateResult = await _clientAppService.UpdateAsync(id, updateRequest);
            if (updateResult.IsFailure)
            {
                SetError(updateResult.Error);
                return;
            }
        }
        else
        {
            var createRequest = new CreateClientRequest(Name, LastName, Rut, Phone, Email, dateOfBirthOnly, Address, Notes);
            var createResult = await _clientAppService.CreateAsync(createRequest);
            if (createResult.IsFailure)
            {
                SetError(createResult.Error);
                return;
            }
        }

        Saved = true;
    });
}
