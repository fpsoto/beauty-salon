using System.Globalization;

namespace Beauty_Salon.Converters;

// Backs a RadioButton-per-enum-value UI: Convert compares the bound enum to
// ConverterParameter; ConvertBack parses ConverterParameter back into the enum when
// this RadioButton becomes checked.
public sealed class EnumToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value?.ToString() == parameter?.ToString();

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true && parameter is not null ? Enum.Parse(targetType, parameter.ToString()!) : BindableProperty.UnsetValue;
}
