using Microsoft.UI;
using Windows.UI;

namespace MuhasibPro.Converters
{
    public static class ColorConverter
    {
        public static Color Parse(string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor) || !hexColor.StartsWith("#"))
                return Colors.DeepSkyBlue;

            try
            {
                hexColor = hexColor.Replace("#", string.Empty);

                if (hexColor.Length == 6)
                {
                    var r = Convert.ToByte(hexColor.Substring(0, 2), 16);
                    var g = Convert.ToByte(hexColor.Substring(2, 2), 16);
                    var b = Convert.ToByte(hexColor.Substring(4, 2), 16);
                    return Color.FromArgb(255, r, g, b);
                }
                else if (hexColor.Length == 8)
                {
                    var a = Convert.ToByte(hexColor.Substring(0, 2), 16);
                    var r = Convert.ToByte(hexColor.Substring(2, 2), 16);
                    var g = Convert.ToByte(hexColor.Substring(4, 2), 16);
                    var b = Convert.ToByte(hexColor.Substring(6, 2), 16);
                    return Color.FromArgb(a, r, g, b);
                }

                return Colors.DeepSkyBlue;
            }
            catch
            {
                return Colors.DeepSkyBlue;
            }
        }
    }
}

