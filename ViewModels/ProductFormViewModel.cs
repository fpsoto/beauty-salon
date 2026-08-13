using BeautySalon.Application.Features.Products;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class ProductFormViewModel : ViewModelBase
{
    private readonly IProductAppService _productAppService;
    private Guid? _productId;

    public ProductFormViewModel(IProductAppService productAppService, ILogger<ProductFormViewModel> logger) : base(logger)
    {
        _productAppService = productAppService;
    }

    [ObservableProperty]
    private string title = "Nuevo producto";

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string? description;

    [ObservableProperty]
    private decimal salePrice;

    [ObservableProperty]
    private bool isActive = true;

    [ObservableProperty]
    private bool saved;

    public void Initialize(ProductDto? existing)
    {
        if (existing is null)
            return;

        _productId = existing.Id;
        Title = "Editar producto";
        Name = existing.Name;
        Description = existing.Description;
        SalePrice = existing.SalePrice;
        IsActive = existing.IsActive;
    }

    [RelayCommand]
    private Task SaveAsync() => SafeExecuteAsync(async () =>
    {
        if (_productId is { } id)
        {
            var result = await _productAppService.UpdateAsync(id, new UpdateProductRequest(Name, Description, SalePrice, IsActive));
            if (result.IsFailure)
            {
                SetError(result.Error);
                return;
            }
        }
        else
        {
            var result = await _productAppService.CreateAsync(new CreateProductRequest(Name, Description, SalePrice));
            if (result.IsFailure)
            {
                SetError(result.Error);
                return;
            }
        }

        Saved = true;
    });
}
