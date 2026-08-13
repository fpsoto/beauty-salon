using Beauty_Salon.ViewModels;

namespace Beauty_Salon.Pages;

[QueryProperty(nameof(ClientId), "ClientId")]
[QueryProperty(nameof(ClientNameParam), "ClientName")]
[QueryProperty(nameof(Balance), "Balance")]
public partial class DebtPaymentFormPage : ContentPage
{
    private readonly DebtPaymentFormViewModel _viewModel;
    private Guid _clientId;
    private string? _clientName;
    private decimal? _balance;

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

    public decimal Balance
    {
        set
        {
            _balance = value;
            TryInitialize();
        }
    }

    public DebtPaymentFormPage(DebtPaymentFormViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.LoadPaymentMethodsCommand.ExecuteAsync(null);
    }

    private void TryInitialize()
    {
        if (_clientName is not null && _balance is not null)
            _viewModel.Initialize(_clientId, _clientName, _balance.Value);
    }

    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DebtPaymentFormViewModel.Saved) && _viewModel.Saved)
            await Shell.Current.GoToAsync("..");
    }
}
