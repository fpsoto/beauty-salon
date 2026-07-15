using Beauty_Salon.ViewModels;
using BeautySalon.Application.Features.Clients;

namespace Beauty_Salon.Pages;

public partial class AppointmentFormPage : ContentPage
{
    private readonly AppointmentFormViewModel _viewModel;

    public AppointmentFormPage(AppointmentFormViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.LoadServicesCommand.ExecuteAsync(null);
    }

    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AppointmentFormViewModel.Created) && _viewModel.Created)
            await Shell.Current.GoToAsync("..");
    }

    private void OnClientSearchTextChanged(object? sender, TextChangedEventArgs e) =>
        _viewModel.SearchClientsCommand.Execute(null);

    private void OnClientSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is ClientDto client)
            _viewModel.SelectClientCommand.Execute(client);
    }

    private void OnServiceCheckedChanged(object? sender, CheckedChangedEventArgs e) =>
        _viewModel.RecomputeTotals();
}
