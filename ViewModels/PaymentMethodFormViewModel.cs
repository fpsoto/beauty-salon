using BeautySalon.Application.Features.Payments;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class PaymentMethodFormViewModel : ViewModelBase
{
    private readonly IPaymentMethodAppService _paymentMethodAppService;
    private Guid? _paymentMethodId;

    public PaymentMethodFormViewModel(IPaymentMethodAppService paymentMethodAppService, ILogger<PaymentMethodFormViewModel> logger) : base(logger)
    {
        _paymentMethodAppService = paymentMethodAppService;
    }

    [ObservableProperty]
    private string title = "Nuevo método de pago";

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private int sortOrder;

    [ObservableProperty]
    private bool isActive = true;

    [ObservableProperty]
    private bool saved;

    public void Initialize(PaymentMethodDto? existing)
    {
        if (existing is null)
            return;

        _paymentMethodId = existing.Id;
        Title = "Editar método de pago";
        Name = existing.Name;
        SortOrder = existing.SortOrder;
        IsActive = existing.IsActive;
    }

    [RelayCommand]
    private Task SaveAsync() => SafeExecuteAsync(async () =>
    {
        if (_paymentMethodId is { } id)
        {
            var result = await _paymentMethodAppService.UpdateAsync(id, new UpdatePaymentMethodRequest(Name, SortOrder, IsActive));
            if (result.IsFailure)
            {
                SetError(result.Error);
                return;
            }
        }
        else
        {
            var result = await _paymentMethodAppService.CreateAsync(new CreatePaymentMethodRequest(Name, SortOrder));
            if (result.IsFailure)
            {
                SetError(result.Error);
                return;
            }
        }

        Saved = true;
    });
}
