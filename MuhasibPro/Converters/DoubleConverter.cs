using Microsoft.UI.Xaml.Data;

namespace MuhasibPro.Converters;

public sealed class DoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double d)
        {
            return d == 0 ? string.Empty : d.ToString();
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value != null)
        {
            if (double.TryParse(value.ToString(), out var d))
            {
                return d;
            }
        }
        return 0.0;
    }
}
