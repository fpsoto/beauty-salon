using System.Collections.ObjectModel;
using BeautySalon.Application.Features.Payments;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class PaymentMethodListViewModel : ViewModelBase
{
    private readonly IPaymentMethodAppService _paymentMethodAppService;

    public PaymentMethodListViewModel(IPaymentMethodAppService paymentMethodAppService, ILogger<PaymentMethodListViewModel> logger) : base(logger)
    {
        _paymentMethodAppService = paymentMethodAppService;
    }

    public ObservableCollection<PaymentMethodDto> PaymentMethods { get; } = [];

    [RelayCommand]
    private Task LoadAsync() => SafeExecuteAsync(LoadCoreAsync);

    [RelayCommand]
    private Task DeleteAsync(PaymentMethodDto method) => SafeExecuteAsync(async () =>
    {
        var result = await _paymentMethodAppService.DeleteAsync(method.Id);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        await LoadCoreAsync();
    });

    private async Task LoadCoreAsync()
    {
        var result = await _paymentMethodAppService.GetAllAsync(false);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        PaymentMethods.Clear();
        foreach (var method in result.Value.OrderBy(m => m.SortOrder))
            PaymentMethods.Add(method);
    }
}
