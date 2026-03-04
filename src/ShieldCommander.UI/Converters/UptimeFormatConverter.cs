using System.Globalization;
using Avalonia.Data.Converters;

namespace ShieldCommander.UI.Converters;

internal sealed class UptimeFormatConverter : IValueConverter
{
    public static readonly UptimeFormatConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TimeSpan ts)
        {
            return "\u2014";
        }

        return ts.Days > 0
            ? $"{ts.Days}d {ts.Hours}:{ts.Minutes:D2}"
            : $"{ts.Hours}:{ts.Minutes:D2}";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
