using CommunityToolkit.Mvvm.ComponentModel;

namespace Beauty_Salon.ViewModels;

// TimePicker binds to TimeSpan (not TimeOnly), so this Presentation-only wrapper
// converts at the edges when loading/saving working hours.
public sealed partial class WorkingDayEditItem : ObservableObject
{
    public required DayOfWeek DayOfWeek { get; init; }
    public required string DayLabel { get; init; }

    [ObservableProperty]
    private bool isWorkingDay;

    [ObservableProperty]
    private TimeSpan startTime;

    [ObservableProperty]
    private TimeSpan endTime;
}
