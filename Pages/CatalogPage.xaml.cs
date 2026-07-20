using Beauty_Salon.Resources.Strings;
using Beauty_Salon.ViewModels;
using BeautySalon.Application.Features.Catalog;

namespace Beauty_Salon.Pages;

public partial class CatalogPage : ContentPage
{
    private readonly CatalogViewModel _viewModel;

    public CatalogPage(CatalogViewModel viewModel)
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

    private async void OnAddCategoryClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("category-form");

    private async void OnAddServiceClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("service-form");

    private async void OnCategoryOptionsTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not ServiceCategoryDto category)
            return;

        var choice = await DisplayActionSheetAsync(category.Name, AppResources.Close, null, AppResources.Edit, AppResources.Delete);

        if (choice == AppResources.Edit)
            await Shell.Current.GoToAsync("category-form", new Dictionary<string, object> { ["Category"] = category });
        else if (choice == AppResources.Delete)
            await _viewModel.DeleteCategoryCommand.ExecuteAsync(category);
    }

    private async void OnServiceSelected(object? sender, SelectionChangedEventArgs e)
    {
        CatalogCollectionView.SelectedItem = null;

        if (e.CurrentSelection.FirstOrDefault() is not SalonServiceDto service)
            return;

        var choice = await DisplayActionSheetAsync(service.Name, AppResources.Close, null, AppResources.Edit, AppResources.Delete);

        if (choice == AppResources.Edit)
            await Shell.Current.GoToAsync("service-form", new Dictionary<string, object> { ["Service"] = service });
        else if (choice == AppResources.Delete)
            await _viewModel.DeleteServiceCommand.ExecuteAsync(service);
    }
}
