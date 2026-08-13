using System.Collections.ObjectModel;
using BeautySalon.Application.Features.Products;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon.ViewModels;

public partial class ProductListViewModel : ViewModelBase
{
    private readonly IProductAppService _productAppService;

    public ProductListViewModel(IProductAppService productAppService, ILogger<ProductListViewModel> logger) : base(logger)
    {
        _productAppService = productAppService;
    }

    public ObservableCollection<ProductDto> Products { get; } = [];

    [RelayCommand]
    private Task LoadAsync() => SafeExecuteAsync(LoadCoreAsync);

    [RelayCommand]
    private Task DeleteAsync(ProductDto product) => SafeExecuteAsync(async () =>
    {
        var result = await _productAppService.DeleteAsync(product.Id);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        await LoadCoreAsync();
    });

    private async Task LoadCoreAsync()
    {
        var result = await _productAppService.GetAllAsync(false);
        if (result.IsFailure)
        {
            SetError(result.Error);
            return;
        }

        Products.Clear();
        foreach (var product in result.Value.OrderBy(p => p.Name))
            Products.Add(product);
    }
}
