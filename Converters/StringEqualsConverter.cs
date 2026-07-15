using System.Globalization;

namespace Beauty_Salon.Converters;

// Same idea as EnumToBoolConverter but for plain string-valued settings (e.g. the
// "es"/"en" language code) instead of an enum.
public sealed class StringEqualsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        string.Equals(value as string, parameter as string, StringComparison.OrdinalIgnoreCase);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true && parameter is not null ? parameter : BindableProperty.UnsetValue;
}
