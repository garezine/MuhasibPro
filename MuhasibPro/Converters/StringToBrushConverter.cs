using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Windows.UI;

namespace MuhasibPro.Converters
{
    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string hexColor && !string.IsNullOrEmpty(hexColor))
            {
                try
                {
                    hexColor = hexColor.Replace("#", string.Empty);

                    if (hexColor.Length == 6)
                    {
                        var r = System.Convert.ToByte(hexColor.Substring(0, 2), 16);
                        var g = System.Convert.ToByte(hexColor.Substring(2, 2), 16);
                        var b = System.Convert.ToByte(hexColor.Substring(4, 2), 16);
                        return new SolidColorBrush(Color.FromArgb(255, r, g, b));
                    }
                    else if (hexColor.Length == 8)
                    {
                        var a = System.Convert.ToByte(hexColor.Substring(0, 2), 16);
                        var r = System.Convert.ToByte(hexColor.Substring(2, 2), 16);
                        var g = System.Convert.ToByte(hexColor.Substring(4, 2), 16);
                        var b = System.Convert.ToByte(hexColor.Substring(6, 2), 16);
                        return new SolidColorBrush(Color.FromArgb(a, r, g, b));
                    }
                }
                catch
                {
                    // Fall through to default
                }
            }

            // Varsayılan renk - mavi
            return new SolidColorBrush(Colors.DeepSkyBlue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}