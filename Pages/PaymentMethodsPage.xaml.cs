using Beauty_Salon.Resources.Strings;
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

        var choice = await DisplayActionSheetAsync(method.Name, AppResources.Close, null, AppResources.Edit, AppResources.Delete);

        if (choice == AppResources.Edit)
            await Shell.Current.GoToAsync("paymentmethods/form", new Dictionary<string, object> { ["PaymentMethod"] = method });
        else if (choice == AppResources.Delete)
            await _viewModel.DeleteCommand.ExecuteAsync(method);
    }
}
