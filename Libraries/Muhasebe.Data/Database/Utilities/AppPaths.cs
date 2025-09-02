using Muhasebe.Data.Database.Utilities;

public static class AppPaths
{
    public static string GetDatabaseDirectory(string preDbName, long dbCode)
    {
        // 1. Firmanın kısa adını güvenli hale getir
        var cleanPreDbName = DatabaseNameGenarator.SanitizeDirectoryName(preDbName);

        // 2. FirmaID'yi Base36 formatında kısalt
        var shortId = IdConverter.ToBase36(dbCode);

        // 3. Ana veri dizinini al
        var basePath = GetBaseDataPath();

        // 4. Tam yolu oluştur: "[BasePath]\Databases\Google_0002P5C1"
        return Path.Combine(basePath, $"{cleanPreDbName}_{shortId}");
    }

    private static string GetBaseDataPath()
    {
        // Örnek: Uygulama dizini altında Data klasörü
        var basePath = Path.Combine(AppContext.BaseDirectory, "Databases");

        // Klasörü oluştur ve özelliklerini ayarla
        Directory.CreateDirectory(basePath);
        File.SetAttributes(basePath, FileAttributes.NotContentIndexed);

        return basePath;
    }
    public static string GetBackupDirectory(string databaseName, long dbCode)
    {
        var cleanDbName = DatabaseNameGenarator.SanitizeDirectoryName(databaseName);
        var shortId = IdConverter.ToBase36(dbCode);
        return Path.Combine(
            GetBaseDataPath(),
            "Backups",
            $"{cleanDbName}_{shortId}",
            DateTime.UtcNow.ToString("yyyy-MM")
        );
    }

    public static string GenerateBackupFileName(string databaseName)
    {
        return $"{databaseName}_{DateTime.UtcNow:yyyyMMddTHHmmss}.bak";
    }
}