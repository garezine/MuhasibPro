using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace MuhasibPro.Converters
{
    public class DatabaseStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isConnected)
            {
                return isConnected
                    ? new SolidColorBrush(Colors.LimeGreen)    // Bağlı - yeşil
                    : new SolidColorBrush(Colors.OrangeRed);   // Bağlı değil - kırmızı
            }

            return new SolidColorBrush(Colors.Gray);           // Bilinmiyor - gri
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
