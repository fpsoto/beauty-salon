using BeautySalon.Application.Features.Catalog;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Beauty_Salon.ViewModels;

public sealed partial class SelectableService : ObservableObject
{
    public required SalonServiceDto Service { get; init; }

    [ObservableProperty]
    private bool isSelected;
}
