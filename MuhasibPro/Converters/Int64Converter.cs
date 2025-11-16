using Microsoft.UI.Xaml.Data;

namespace MuhasibPro.Converters;

public sealed class Int64Converter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is long n64)
        {
            if (targetType == typeof(string))
            {
                return n64 == 0L ? string.Empty : n64.ToString();
            }
            return n64;
        }
        if (targetType == typeof(string))
        {
            return string.Empty;
        }
        return 0L;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value != null)
        {
            if (long.TryParse(value.ToString(), out var n64))
            {
                return n64;
            }
        }
        return 0L;
    }
}
