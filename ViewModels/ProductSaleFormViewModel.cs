using System.Collections.ObjectModel;
using Beauty_Salon.Resources.Strings;
using BeautySalon.Application.Common;
using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Application.Features.Clients;
using BeautySalon.Application.Features.Payments;
using BeautySalon.Application.Features.Products;
using BeautySalon.Application.Features.Sales;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class ProductSaleFormViewModel : ViewModelBase
{
    private readonly IProductSaleAppService _productSaleAppService;
    private readonly IProductAppService _productAppService;
    private readonly IPaymentMethodAppService _paymentMethodAppService;
    private readonly IClientAppService _clientAppService;
    private readonly ICurrentUserContext _currentUserContext;

    private Guid? _saleId;
    private Guid? _existingProductId;
    private Guid? _existingPaymentMethodId;
    private Guid? _existingClientId;

    // Falls back to the seeded admin only as a defensive default - this page is only
    // reachable post-login, so _currentUserContext.UserId should always be set by then.
    private Guid ProfessionalId => _currentUserContext.UserId ?? WellKnownIds.AdminUserId;

    public ProductSaleFormViewModel(
        IProductSaleAppService productSaleAppService,
        IProductAppService productAppService,
        IPaymentMethodAppService paymentMethodAppService,
        IClientAppService clientAppService,
        ICurrentUserContext currentUserContext,
        ILogger<ProductSaleFormViewModel> logger) : base(logger)
    {
        _productSaleAppService = productSaleAppService;
        _productAppService = productAppService;
        _paymentMethodAppService = paymentMethodAppService;
        _clientAppService = clientAppService;
        _currentUserContext = currentUserContext;
        SaleDate = DateTime.Today;
    }

    [ObservableProperty]
    private string title = "Nueva venta";

    [ObservableProperty]
    private ProductDto? selectedProduct;

    [ObservableProperty]
    private int quantity = 1;

    [ObservableProperty]
    private decimal unitPrice;

    [ObservableProperty]
    private string clientSearchTerm = string.Empty;

    [ObservableProperty]
    private ClientDto? selectedClient;

    [ObservableProperty]
    private PaymentMethodDto? selectedPaymentMethod;

    [ObservableProperty]
    private DateTime saleDate;

    [ObservableProperty]
    private bool saved;

    public ObservableCollection<ProductDto> Products { get; } = [];
    public ObservableCollection<PaymentMethodDto> PaymentMethods { get; } = [];
    public ObservableCollection<ClientDto> ClientResults { get; } = [];

    public decimal Total => Quantity * UnitPrice;

    partial void OnSelectedProductChanged(ProductDto? value)
    {
        // Auto-fills the suggested catalog price on a new sale, still editable
        // afterward - the same "suggested vs charged" latitude Appointment allows.
        // Never overwrites an already-recorded historical price when editing.
        if (value is not null && _saleId is null)
            UnitPrice = value.SalePrice;

        OnPropertyChanged(nameof(Total));
    }

    partial void OnQuantityChanged(int value) => OnPropertyChanged(nameof(Total));

    partial void OnUnitPriceChanged(decimal value) => OnPropertyChanged(nameof(Total));

    public void Initialize(ProductSaleDto? existing)
    {
        if (existing is null)
            return;

        _saleId = existing.Id;
        _existingProductId = existing.ProductId;
        _existingPaymentMethodId = existing.PaymentMethodId;
        _existingClientId = existing.ClientId;
        Title = "Editar venta";
        Quantity = existing.Quantity;
        UnitPrice = existing.UnitPrice;
        SaleDate = existing.SaleDate.ToDateTime(TimeOnly.MinValue);
        if (existing.ClientId is not null)
            ClientSearchTerm = existing.ClientFullName ?? string.Empty;
    }

    [RelayCommand]
    private Task LoadLookupsAsync() => SafeExecuteAsync(async () =>
    {
        var productsResult = await _productAppService.GetAllAsync(true);
        if (productsResult.IsFailure)
        {
            SetError(productsResult.Error);
            return;
        }

        Products.Clear();
        foreach (var product in productsResult.Value)
            Products.Add(product);

        if (_existingProductId is { } productId)
            SelectedProduct = Products.FirstOrDefault(p => p.Id == productId);

        var paymentMethodsResult = await _paymentMethodAppService.GetAllAsync(true);
        if (paymentMethodsResult.IsFailure)
        {
            SetError(paymentMethodsResult.Error);
            return;
        }

        PaymentMethods.Clear();
        foreach (var method in paymentMethodsResult.Value)
            PaymentMethods.Add(method);

        if (_existingPaymentMethodId is { } paymentMethodId)
            SelectedPaymentMethod = PaymentMethods.FirstOrDefault(m => m.Id == paymentMethodId);

        if (_existingClientId is { } clientId)
        {
            var clientResult = await _clientAppService.GetDetailAsync(clientId);
            if (clientResult.IsSuccess)
                SelectedClient = clientResult.Value.Client;
        }
    });

    [RelayCommand]
    private Task SearchClientsAsync() => SafeExecuteAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(ClientSearchTerm) || SelectedClient is not null)
        {
            ClientResults.Clear();
            return;
        }

        var result = await _clientAppService.SearchAsync(ClientSearchTerm);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        ClientResults.Clear();
        foreach (var client in result.Value)
            ClientResults.Add(client);
    });

    [RelayCommand]
    private void SelectClient(ClientDto client)
    {
        SelectedClient = client;
        ClientResults.Clear();
        ClientSearchTerm = $"{client.Name} {client.LastName}";
    }

    [RelayCommand]
    private void ClearSelectedClient()
    {
        SelectedClient = null;
        ClientSearchTerm = string.Empty;
    }

    [RelayCommand]
    private Task SaveAsync() => SafeExecuteAsync(async () =>
    {
        if (SelectedProduct is null)
        {
            ErrorMessage = AppResources.SelectProductRequired;
            return;
        }

        if (SelectedPaymentMethod is null)
        {
            ErrorMessage = AppResources.SelectPaymentMethodRequired;
            return;
        }

        if (_saleId is { } id)
        {
            var request = new UpdateProductSaleRequest(
                Quantity, UnitPrice, SelectedClient?.Id, SelectedPaymentMethod.Id, DateOnly.FromDateTime(SaleDate));
            var result = await _productSaleAppService.UpdateAsync(id, request);
            if (result.IsFailure)
            {
                SetError(result.Error);
                return;
            }
        }
        else
        {
            var request = new CreateProductSaleRequest(
                SelectedProduct.Id, Quantity, UnitPrice, SelectedClient?.Id, SelectedPaymentMethod.Id, DateOnly.FromDateTime(SaleDate), ProfessionalId);
            var result = await _productSaleAppService.CreateAsync(request);
            if (result.IsFailure)
            {
                SetError(result.Error);
                return;
            }
        }

        Saved = true;
    });
}
