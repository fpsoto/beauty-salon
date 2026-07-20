using Beauty_Salon.ViewModels;
using BeautySalon.Application.Features.Clients;

namespace Beauty_Salon.Pages;

public partial class ClientListPage : ContentPage
{
    private readonly ClientListViewModel _viewModel;

    public ClientListPage(ClientListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.SearchCommand.ExecuteAsync(null);
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e) =>
        _viewModel.SearchCommand.Execute(null);

    private async void OnClientSelected(object? sender, SelectionChangedEventArgs e)
    {
        ClientsCollectionView.SelectedItem = null;

        if (e.CurrentSelection.FirstOrDefault() is ClientDto client)
            await Shell.Current.GoToAsync("client-detail", new Dictionary<string, object> { ["ClientId"] = client.Id });
    }

    private void OnFavoriteTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is ClientDto client)
            _viewModel.ToggleFavoriteCommand.Execute(client);
    }

    private async void OnAddClientClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("client-form");
}
