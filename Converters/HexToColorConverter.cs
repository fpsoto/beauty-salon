using System.Globalization;

namespace Beauty_Salon.Converters;

public sealed class HexToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string hex && !string.IsNullOrWhiteSpace(hex) ? Color.FromArgb(hex) : Colors.Gray;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
