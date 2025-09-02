using Microsoft.UI.Xaml.Data;

namespace MuhasibPro.Infrastructure.Infrastructure.Converters;

public sealed class Int16Converter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is short n16)
        {
            if (targetType == typeof(string))
            {
                return n16 == 0 ? "" : n16.ToString();
            }
            return n16;
        }
        if (targetType == typeof(string))
        {
            return "";
        }
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value != null)
        {
            if (short.TryParse(value.ToString(), out var n16))
            {
                return n16;
            }
        }
        return 0;
    }
}
