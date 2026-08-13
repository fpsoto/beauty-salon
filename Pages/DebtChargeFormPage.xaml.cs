using Beauty_Salon.ViewModels;

namespace Beauty_Salon.Pages;

[QueryProperty(nameof(ClientId), "ClientId")]
[QueryProperty(nameof(ClientNameParam), "ClientName")]
public partial class DebtChargeFormPage : ContentPage
{
    private readonly DebtChargeFormViewModel _viewModel;
    private Guid _clientId;
    private string? _clientName;

    public Guid ClientId
    {
        set
        {
            _clientId = value;
            TryInitialize();
        }
    }

    public string ClientNameParam
    {
        set
        {
            _clientName = value;
            TryInitialize();
        }
    }

    public DebtChargeFormPage(DebtChargeFormViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void TryInitialize()
    {
        if (_clientName is not null)
            _viewModel.Initialize(_clientId, _clientName);
    }

    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DebtChargeFormViewModel.Saved) && _viewModel.Saved)
            await Shell.Current.GoToAsync("..");
    }
}
