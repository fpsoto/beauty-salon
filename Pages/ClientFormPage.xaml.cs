using Beauty_Salon.ViewModels;

namespace Beauty_Salon.Pages;

[QueryProperty(nameof(ClientId), "ClientId")]
public partial class ClientFormPage : ContentPage
{
    private readonly ClientFormViewModel _viewModel;

    public Guid ClientId
    {
        set => _viewModel.Initialize(value);
    }

    public ClientFormPage(ClientFormViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ClientFormViewModel.Saved) && _viewModel.Saved)
            await Shell.Current.GoToAsync("..");
    }
}
