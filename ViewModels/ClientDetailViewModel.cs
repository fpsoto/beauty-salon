using BeautySalon.Application.Features.Clients;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class ClientDetailViewModel : ViewModelBase
{
    private readonly IClientAppService _clientAppService;

    public ClientDetailViewModel(IClientAppService clientAppService, ILogger<ClientDetailViewModel> logger) : base(logger)
    {
        _clientAppService = clientAppService;
    }

    [ObservableProperty]
    private ClientDetailDto? detail;

    public Task LoadAsync(Guid clientId) => SafeExecuteAsync(async () =>
    {
        var result = await _clientAppService.GetDetailAsync(clientId);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        Detail = result.Value;
    });

    [RelayCommand]
    private Task ToggleFavoriteAsync() => SafeExecuteAsync(async () =>
    {
        if (Detail is null)
            return;

        var result = await _clientAppService.SetFavoriteAsync(Detail.Client.Id, !Detail.Client.IsFavorite);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        var refreshed = await _clientAppService.GetDetailAsync(Detail.Client.Id);
        if (refreshed.IsSuccess)
            Detail = refreshed.Value;
    });

    [RelayCommand]
    private void Call()
    {
        if (Detail is null)
            return;

        PhoneDialer.Default.Open(Detail.Client.Phone);
    }

    [RelayCommand]
    private async Task SendWhatsAppAsync()
    {
        if (Detail is null)
            return;

        // wa.me expects digits only (country code + number, no "+"/spaces/dashes).
        var digitsOnly = new string(Detail.Client.Phone.Where(char.IsDigit).ToArray());
        await Launcher.Default.OpenAsync(new Uri($"https://wa.me/{digitsOnly}"));
    }

    [RelayCommand]
    private async Task SendEmailAsync()
    {
        if (Detail?.Client.Email is not { Length: > 0 } email)
            return;

        await Launcher.Default.OpenAsync(new Uri($"mailto:{email}"));
    }
}
