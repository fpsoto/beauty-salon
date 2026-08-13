using Beauty_Salon.Resources.Strings;
using Beauty_Salon.ViewModels;
using BeautySalon.Application.Features.Products;

namespace Beauty_Salon.Pages;

public partial class ProductListPage : ContentPage
{
    private readonly ProductListViewModel _viewModel;

    public ProductListPage(ProductListViewModel viewModel)
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

    private async void OnAddClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("product-form");

    private async void OnProductSelected(object? sender, SelectionChangedEventArgs e)
    {
        ProductsCollectionView.SelectedItem = null;

        if (e.CurrentSelection.FirstOrDefault() is not ProductDto product)
            return;

        var choice = await DisplayActionSheetAsync(product.Name, AppResources.Close, null, AppResources.Edit, AppResources.Delete);

        if (choice == AppResources.Edit)
            await Shell.Current.GoToAsync("product-form", new Dictionary<string, object> { ["Product"] = product });
        else if (choice == AppResources.Delete)
        {
            var confirmed = await DisplayAlertAsync(product.Name, AppResources.DeleteConfirmMessage, AppResources.Delete, AppResources.Close);
            if (confirmed)
                await _viewModel.DeleteCommand.ExecuteAsync(product);
        }
    }
}
