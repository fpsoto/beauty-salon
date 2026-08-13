using Beauty_Salon.ViewModels;
using BeautySalon.Application.Features.Debts;

namespace Beauty_Salon.Pages;

public partial class ClientDebtsPage : ContentPage
{
    private readonly ClientDebtsViewModel _viewModel;

    public ClientDebtsPage(ClientDebtsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.LoadCommand.ExecuteAsync(null);
    }

    private async void OnClientSelected(object? sender, SelectionChangedEventArgs e)
    {
        BalancesCollectionView.SelectedItem = null;

        if (e.CurrentSelection.FirstOrDefault() is not ClientBalanceDto balance)
            return;

        await Shell.Current.GoToAsync("client-detail", new Dictionary<string, object> { ["ClientId"] = balance.ClientId });
    }
}
