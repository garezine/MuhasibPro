using System.Text;

namespace Muhasebe.Data.Database.Utilities
{
    // EnvanterPro.Common.Utilities/IdConverter.cs
    public static class IdConverter
    {
        private const string Base36Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string ToBase36(this long input)
        {
            var result = new StringBuilder();
            while (input > 0)
            {
                result.Insert(0, Base36Chars[(int)(input % 36)]);
                input /= 36;
            }
            return result.ToString().PadLeft(8, '0'); // 8 karakter sabit uzunluk
        }
    }
}
