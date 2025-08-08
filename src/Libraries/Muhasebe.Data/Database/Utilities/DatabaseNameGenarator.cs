namespace Muhasebe.Data.Database.Utilities
{
    public static class DatabaseNameGenarator
    {
        public static string Generate(string preDbName)
        {
            // Örnek: "Korkut Mermer" -> "KM"
            var text = preDbName.Split(' ');
            return text.Length switch
            {
                0 => "FN", // Fallback Name
                1 => preDbName[..2].ToUpper(),
                _ => $"{text[0][0]}{text[1][0]}".ToUpper()
            };
        }
        public static string SanitizeDirectoryName(string rawName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(rawName
                .Where(c => !invalidChars.Contains(c))
                .ToArray());
            return sanitized.Trim();
        }
        public static string GenerateDatabaseName(string preDbName, int createdDate)
        {
            // 1. Firmanın kısa kodunu üret (Örn: "Korkut Mermer" → "KM")
            var dbCodeName = Generate(preDbName);

            // 2. Güvenli veritabanı adı oluştur (özel karakterleri temizle)
            var newDbCodeName = SanitizeDirectoryName(dbCodeName);

            // 3. Format: "KM_2024"
            return $"{newDbCodeName}_{createdDate}";
        }
    }
}

