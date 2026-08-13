using Beauty_Salon.ViewModels;
using BeautySalon.Application.Features.Clients;
using BeautySalon.Application.Features.Sales;

namespace Beauty_Salon.Pages;

[QueryProperty(nameof(ProductSale), "ProductSale")]
public partial class ProductSaleFormPage : ContentPage
{
    private readonly ProductSaleFormViewModel _viewModel;

    public ProductSaleDto? ProductSale
    {
        set => _viewModel.Initialize(value);
    }

    public ProductSaleFormPage(ProductSaleFormViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.LoadLookupsCommand.ExecuteAsync(null);
    }

    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProductSaleFormViewModel.Saved) && _viewModel.Saved)
            await Shell.Current.GoToAsync("..");
    }

    private void OnClientSearchTextChanged(object? sender, TextChangedEventArgs e) =>
        _viewModel.SearchClientsCommand.Execute(null);

    private void OnClientSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is ClientDto client)
            _viewModel.SelectClientCommand.Execute(client);
    }
}
