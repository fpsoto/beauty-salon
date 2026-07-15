using System.Collections.ObjectModel;
using BeautySalon.Application.Features.Clients;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class ClientListViewModel : ViewModelBase
{
    private readonly IClientAppService _clientAppService;

    public ClientListViewModel(IClientAppService clientAppService, ILogger<ClientListViewModel> logger) : base(logger)
    {
        _clientAppService = clientAppService;
    }

    [ObservableProperty]
    private string searchTerm = string.Empty;

    public ObservableCollection<ClientDto> Clients { get; } = [];

    // Empty search shows favorites first - keeps the most-used clients one tap away.
    [RelayCommand]
    private Task SearchAsync() => SafeExecuteAsync(SearchCoreAsync);

    [RelayCommand]
    private Task ToggleFavoriteAsync(ClientDto client) => SafeExecuteAsync(async () =>
    {
        var result = await _clientAppService.SetFavoriteAsync(client.Id, !client.IsFavorite);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        await SearchCoreAsync();
    });

    private async Task SearchCoreAsync()
    {
        var result = string.IsNullOrWhiteSpace(SearchTerm)
            ? await _clientAppService.GetFavoritesAsync()
            : await _clientAppService.SearchAsync(SearchTerm);

        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        Clients.Clear();
        foreach (var client in result.Value)
            Clients.Add(client);
    }
}
