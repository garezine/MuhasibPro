
using Microsoft.UI.Xaml.Data;

namespace MuhasibPro.Core.Infrastructure.Converters;

public sealed class Int32Converter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int n32)
        {
            if (targetType == typeof(string))
            {
                return n32 == 0 ? "" : n32.ToString();
            }
            return n32;
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
            if (int.TryParse(value.ToString(), out var n32))
            {
                return n32;
            }
        }
        return 0;
    }
}
