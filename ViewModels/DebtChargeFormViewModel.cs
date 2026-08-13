using BeautySalon.Application.Common;
using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Application.Features.Debts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class DebtChargeFormViewModel : ViewModelBase
{
    private readonly IClientDebtAppService _clientDebtAppService;
    private readonly ICurrentUserContext _currentUserContext;

    private Guid _clientId;

    private Guid ProfessionalId => _currentUserContext.UserId ?? WellKnownIds.AdminUserId;

    public DebtChargeFormViewModel(IClientDebtAppService clientDebtAppService, ICurrentUserContext currentUserContext, ILogger<DebtChargeFormViewModel> logger) : base(logger)
    {
        _clientDebtAppService = clientDebtAppService;
        _currentUserContext = currentUserContext;
        EntryDate = DateTime.Today;
    }

    [ObservableProperty] private string clientName = string.Empty;
    [ObservableProperty] private decimal amount;
    [ObservableProperty] private string? description;
    [ObservableProperty] private DateTime entryDate;
    [ObservableProperty] private bool saved;

    public void Initialize(Guid clientId, string clientName)
    {
        _clientId = clientId;
        ClientName = clientName;
    }

    [RelayCommand]
    private Task SaveAsync() => SafeExecuteAsync(async () =>
    {
        var request = new CreateChargeRequest(_clientId, Amount, Description, DateOnly.FromDateTime(EntryDate), ProfessionalId);
        var result = await _clientDebtAppService.AddChargeAsync(request);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        Saved = true;
    });
}
