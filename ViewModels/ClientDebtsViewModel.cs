using System.Collections.ObjectModel;
using BeautySalon.Application.Features.Debts;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class ClientDebtsViewModel : ViewModelBase
{
    private readonly IClientDebtAppService _clientDebtAppService;

    public ClientDebtsViewModel(IClientDebtAppService clientDebtAppService, ILogger<ClientDebtsViewModel> logger) : base(logger)
    {
        _clientDebtAppService = clientDebtAppService;
    }

    public ObservableCollection<ClientBalanceDto> Balances { get; } = [];

    [RelayCommand]
    private Task LoadAsync() => SafeExecuteAsync(async () =>
    {
        var result = await _clientDebtAppService.GetOutstandingBalancesAsync();
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        Balances.Clear();
        foreach (var balance in result.Value)
            Balances.Add(balance);
    });
}
