using Muhasebe.Domain.Enum;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Muhasebe.Domain.Helpers
{
    public static class ResultCodeExtensions
    {
        public static string GetMessage(this ResultCodes code, params object[] args)
        {
            var field = code.GetType().GetField(code.ToString());
            if (field == null)
            {
                return code.ToString();
            }

            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));

            if (attribute != null)
            {
                string description = attribute.Description;

                // Description string'inde {0}, {1} gibi format placeholder'ları var mı kontrol et
                bool hasFormatPlaceholders = Regex.IsMatch(description, @"\{\d+\}");

                // Eğer placeholder'lar varsa VE argüman sağlanmamışsa (null veya boş dizi)
                if (hasFormatPlaceholders && (args == null || args.Length == 0))
                {
                    // İşte burası kritik nokta: Varsayılan bir isim atıyoruz.
                    // Örneğin "Öğe", "Kayıt", "İşlem" veya "Veri" gibi genel bir ifade kullanabilirsiniz.
                    // Ben burada "İşlem" olarak varsayılan bir değer verdim.
                    string defaultName = "Veri";
                    return string.Format(description, defaultName);
                }
                else
                {
                    // Argümanlar sağlandıysa veya description'da placeholder yoksa formatla
                    return string.Format(description, args);
                }
            }
            else
            {
                // Description attribute'ı yoksa, enum adını string olarak döndür
                return code.ToString();
            }
        }
    }
}
