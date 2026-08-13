using System.Collections.ObjectModel;
using Beauty_Salon.Resources.Strings;
using BeautySalon.Application.Common;
using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Application.Features.Debts;
using BeautySalon.Application.Features.Payments;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class DebtPaymentFormViewModel : ViewModelBase
{
    private readonly IClientDebtAppService _clientDebtAppService;
    private readonly IPaymentMethodAppService _paymentMethodAppService;
    private readonly ICurrentUserContext _currentUserContext;

    private Guid _clientId;

    private Guid ProfessionalId => _currentUserContext.UserId ?? WellKnownIds.AdminUserId;

    public DebtPaymentFormViewModel(
        IClientDebtAppService clientDebtAppService,
        IPaymentMethodAppService paymentMethodAppService,
        ICurrentUserContext currentUserContext,
        ILogger<DebtPaymentFormViewModel> logger) : base(logger)
    {
        _clientDebtAppService = clientDebtAppService;
        _paymentMethodAppService = paymentMethodAppService;
        _currentUserContext = currentUserContext;
        EntryDate = DateTime.Today;
    }

    [ObservableProperty] private string clientName = string.Empty;
    [ObservableProperty] private decimal balance;
    [ObservableProperty] private decimal amount;
    [ObservableProperty] private PaymentMethodDto? selectedPaymentMethod;
    [ObservableProperty] private DateTime entryDate;
    [ObservableProperty] private bool saved;

    public ObservableCollection<PaymentMethodDto> PaymentMethods { get; } = [];

    public void Initialize(Guid clientId, string clientName, decimal balance)
    {
        _clientId = clientId;
        ClientName = clientName;
        Balance = balance;
    }

    [RelayCommand]
    private Task LoadPaymentMethodsAsync() => SafeExecuteAsync(async () =>
    {
        var result = await _paymentMethodAppService.GetAllAsync(true);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        PaymentMethods.Clear();
        foreach (var method in result.Value)
            PaymentMethods.Add(method);
    });

    [RelayCommand]
    private Task SaveAsync() => SafeExecuteAsync(async () =>
    {
        if (SelectedPaymentMethod is null)
        {
            ErrorMessage = AppResources.SelectPaymentMethodRequired;
            return;
        }

        var request = new CreatePaymentRequest(_clientId, Amount, SelectedPaymentMethod.Id, DateOnly.FromDateTime(EntryDate), ProfessionalId);
        var result = await _clientDebtAppService.AddPaymentAsync(request);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        Saved = true;
    });
}
