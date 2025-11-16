using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Muhasib.Domain.Enum;

namespace MuhasibPro.Converters;

public sealed class LogTypeConverter : IValueConverter
{
    private readonly SolidColorBrush InformationColor = new SolidColorBrush(Colors.DeepSkyBlue);
    private readonly SolidColorBrush WarningColor = new SolidColorBrush(Colors.Gold);
    private readonly SolidColorBrush ErrorColor = new SolidColorBrush(Colors.IndianRed);

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (targetType == typeof(string))
        {
            if (value is LogType logType)
            {
                switch (logType)
                {
                    case LogType.Bilgi:
                        return char.ConvertFromUtf32(0xE946).ToString();
                    case LogType.Dikkat:
                        return char.ConvertFromUtf32(0xE814).ToString();
                    case LogType.Hata:
                        return char.ConvertFromUtf32(0xEB90).ToString();
                }
            }
            return char.ConvertFromUtf32(0xE946).ToString();
        }

        if (targetType == typeof(Brush))
        {
            if (value is LogType logType)
            {
                switch (logType)
                {
                    case LogType.Bilgi:
                        return InformationColor;
                    case LogType.Dikkat:
                        return WarningColor;
                    case LogType.Hata:
                        return ErrorColor;
                }
            }
            return InformationColor;
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
