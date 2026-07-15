using System.Collections.ObjectModel;
using BeautySalon.Application.Features.Payments;
using BeautySalon.Application.Features.Schedule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class FinishAppointmentViewModel : ViewModelBase
{
    private readonly IAppointmentAppService _appointmentAppService;
    private readonly IPaymentMethodAppService _paymentMethodAppService;
    private Guid _appointmentId;

    public FinishAppointmentViewModel(
        IAppointmentAppService appointmentAppService,
        IPaymentMethodAppService paymentMethodAppService,
        ILogger<FinishAppointmentViewModel> logger) : base(logger)
    {
        _appointmentAppService = appointmentAppService;
        _paymentMethodAppService = paymentMethodAppService;
    }

    [ObservableProperty]
    private decimal chargedPrice;

    [ObservableProperty]
    private decimal discount;

    [ObservableProperty]
    private decimal tip;

    [ObservableProperty]
    private string? notes;

    [ObservableProperty]
    private PaymentMethodDto? selectedPaymentMethod;

    [ObservableProperty]
    private bool finished;

    public ObservableCollection<PaymentMethodDto> PaymentMethods { get; } = [];

    public void Initialize(Guid appointmentId) => _appointmentId = appointmentId;

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
            ErrorMessage = "Selecciona un método de pago.";
            return;
        }

        var request = new FinishAppointmentRequest(_appointmentId, ChargedPrice, Discount, Tip, SelectedPaymentMethod.Id, Notes);
        var result = await _appointmentAppService.FinishAsync(request);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        Finished = true;
    });
}
