using Windows.Foundation;

namespace MuhasibPro.Extensions;

public static class UriExtensions
{
    public static long GetInt64Parameter(this Uri uri, string name)
    {
        var value = uri.GetParameter(name);
        if (value != null)
        {
            if (long.TryParse(value, out var n))
            {
                return n;
            }
        }
        return 0;
    }

    public static int GetInt32Parameter(this Uri uri, string name)
    {
        var value = uri.GetParameter(name);
        if (value != null)
        {
            if (int.TryParse(value, out var n))
            {
                return n;
            }
        }
        return 0;
    }

    public static string GetParameter(this Uri uri, string name)
    {
        var query = uri.Query;
        if (!string.IsNullOrEmpty(query))
        {
            try
            {
                var decoder = new WwwFormUrlDecoder(uri.Query);
                return decoder.GetFirstValueByName("id");
            }
            catch { }
        }
        return null;
    }
}
