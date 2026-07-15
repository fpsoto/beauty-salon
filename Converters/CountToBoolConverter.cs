using System.Globalization;

namespace Beauty_Salon.Converters;

// IsVisible bound to a plain "{Binding SomeCollection}" never refreshes when the
// collection's contents change (only ObservableCollection.Count/Item[] raise
// PropertyChanged) - bind to "SomeCollection.Count" with this converter instead.
public sealed class CountToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int count && count > 0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
