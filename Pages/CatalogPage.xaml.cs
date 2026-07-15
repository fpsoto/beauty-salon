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
        await Shell.Current.GoToAsync("catalog/category-form");

    private async void OnAddServiceClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("catalog/service-form");

    private async void OnCategoryOptionsTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not ServiceCategoryDto category)
            return;

        var choice = await DisplayActionSheetAsync(category.Name, "Cerrar", null, "Editar", "Eliminar");

        switch (choice)
        {
            case "Editar":
                await Shell.Current.GoToAsync("catalog/category-form", new Dictionary<string, object> { ["Category"] = category });
                break;
            case "Eliminar":
                await _viewModel.DeleteCategoryCommand.ExecuteAsync(category);
                break;
        }
    }

    private async void OnServiceSelected(object? sender, SelectionChangedEventArgs e)
    {
        CatalogCollectionView.SelectedItem = null;

        if (e.CurrentSelection.FirstOrDefault() is not SalonServiceDto service)
            return;

        var choice = await DisplayActionSheetAsync(service.Name, "Cerrar", null, "Editar", "Eliminar");

        switch (choice)
        {
            case "Editar":
                await Shell.Current.GoToAsync("catalog/service-form", new Dictionary<string, object> { ["Service"] = service });
                break;
            case "Eliminar":
                await _viewModel.DeleteServiceCommand.ExecuteAsync(service);
                break;
        }
    }
}
