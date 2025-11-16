using Microsoft.UI.Xaml.Data;
using Windows.Globalization.DateTimeFormatting;
using Windows.System.UserProfile;

namespace MuhasibPro.Converters;

public sealed class DateTimeFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        try
        {
            if (value is DateTime dateTime)
            {
                if (dateTime == DateTime.MinValue)
                {
                    return string.Empty;
                }
                value = new DateTimeOffset(dateTime);
            }
            if (value is DateTimeOffset dateTimeOffset)
            {
                var format = parameter as string ?? "shortdate";
                var userLanguages = GlobalizationPreferences.Languages;
                var dateFormatter = new DateTimeFormatter(format, userLanguages);
                return dateFormatter.Format(dateTimeOffset.ToLocalTime());
            }
            return "N/A";
        }
        catch
        {
            return string.Empty;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
