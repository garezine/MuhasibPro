using Windows.Foundation;

namespace MuhasibPro.Extensions
{
    public static class UriExtensions
    {
        public static Int64 GetInt64Parameter(this Uri uri, string name)
        {
            string value = GetParameter(uri, name);
            if (value != null)
            {
                if (Int64.TryParse(value, out Int64 n))
                {
                    return n;
                }
            }
            return 0;
        }

        public static Int32 GetInt32Parameter(this Uri uri, string name)
        {
            string value = GetParameter(uri, name);
            if (value != null)
            {
                if (Int32.TryParse(value, out Int32 n))
                {
                    return n;
                }
            }
            return 0;
        }

        public static string GetParameter(this Uri uri, string name)
        {
            string query = uri.Query;
            if (!String.IsNullOrEmpty(query))
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
}
