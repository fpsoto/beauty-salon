using Beauty_Salon.ViewModels;
using BeautySalon.Application.Features.Payments;

namespace Beauty_Salon.Pages;

public partial class PaymentMethodsPage : ContentPage
{
    private readonly PaymentMethodListViewModel _viewModel;

    public PaymentMethodsPage(PaymentMethodListViewModel viewModel)
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
        await Shell.Current.GoToAsync("paymentmethods/form");

    private async void OnMethodSelected(object? sender, SelectionChangedEventArgs e)
    {
        PaymentMethodsCollectionView.SelectedItem = null;

        if (e.CurrentSelection.FirstOrDefault() is not PaymentMethodDto method)
            return;

        var choice = await DisplayActionSheetAsync(method.Name, "Cerrar", null, "Editar", "Eliminar");

        switch (choice)
        {
            case "Editar":
                await Shell.Current.GoToAsync("paymentmethods/form", new Dictionary<string, object> { ["PaymentMethod"] = method });
                break;
            case "Eliminar":
                await _viewModel.DeleteCommand.ExecuteAsync(method);
                break;
        }
    }
}
