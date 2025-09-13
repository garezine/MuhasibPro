using Microsoft.UI.Xaml.Data;

namespace MuhasibPro.Converters
{
    public class GlyphToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string glyph && !string.IsNullOrEmpty(glyph))
            {
                return new FontIcon { Glyph = glyph };
            }
            return new SymbolIcon(Symbol.Home);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
