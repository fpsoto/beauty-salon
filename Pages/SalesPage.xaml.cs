using Beauty_Salon.Resources.Strings;
using Beauty_Salon.ViewModels;
using BeautySalon.Application.Features.Sales;

namespace Beauty_Salon.Pages;

public partial class SalesPage : ContentPage
{
    private readonly SalesViewModel _viewModel;

    public SalesPage(SalesViewModel viewModel)
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

    private async void OnAddSaleClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("productsale-form");

    private async void OnProductsClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("product-list");

    private async void OnSaleSelected(object? sender, SelectionChangedEventArgs e)
    {
        SalesCollectionView.SelectedItem = null;

        if (e.CurrentSelection.FirstOrDefault() is not ProductSaleDto sale)
            return;

        var choice = await DisplayActionSheetAsync(sale.ProductName, AppResources.Close, null, AppResources.Edit, AppResources.Delete);

        if (choice == AppResources.Edit)
            await Shell.Current.GoToAsync("productsale-form", new Dictionary<string, object> { ["ProductSale"] = sale });
        else if (choice == AppResources.Delete)
        {
            var confirmed = await DisplayAlertAsync(sale.ProductName, AppResources.DeleteConfirmMessage, AppResources.Delete, AppResources.Close);
            if (confirmed)
                await _viewModel.DeleteCommand.ExecuteAsync(sale);
        }
    }
}
