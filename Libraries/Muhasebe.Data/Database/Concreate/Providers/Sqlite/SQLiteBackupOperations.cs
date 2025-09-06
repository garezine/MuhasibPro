using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Muhasebe.Data.Database.Helpers;
using Muhasebe.Data.Database.Interfaces.Operations;

namespace Muhasebe.Data.Database.Concreate.Providers.Sqlite
{
    // --- SQLite Yedekleme ---
    public class SQLiteBackupOperations : IDatabaseBackupOperations
    {
        private readonly ILogger<SQLiteBackupOperations> _logger;

        public SQLiteBackupOperations(ILogger<SQLiteBackupOperations> logger)
        {
            _logger = logger;
        }

        public async Task<BackupResult> BackupDatabaseAsync(string connectionString, string dbName, string dbPath, string backupFilePath)
        {
            _logger.LogInformation("Starting SQLite backup for database file '{DbPath}' to '{BackupFilePath}'...", dbPath, backupFilePath);

            if (!File.Exists(dbPath))
            {
                _logger.LogError("Source SQLite database file not found: {DbPath}", dbPath);
                return new BackupResult(false, "Kaynak veritabanı dosyası bulunamadı.", null);
            }

            try
            {
                // En basit yöntem: Dosya kopyalama.
                // Daha güvenli yöntem: SQLite Online Backup API kullanmak.
                // File.Copy(dbPath, backupFilePath, true); // Overwrite=true

                // Online Backup API örneği (Microsoft.Data.Sqlite):
                // Bu, DB kullanımdayken bile tutarlı yedek alır.
                var sourceConnectionString = $"Data Source={dbPath}"; // Veya verilen connectionString parse edilebilir
                var backupConnectionString = $"Data Source={backupFilePath}";

                // Hedef dosya varsa sil (Backup API üzerine yazmaz)
                if (File.Exists(backupFilePath))
                {
                    File.Delete(backupFilePath);
                    _logger.LogDebug("Deleted existing target backup file: {BackupFilePath}", backupFilePath);
                }

                using (var sourceConnection = new SqliteConnection(sourceConnectionString))
                using (var backupConnection = new SqliteConnection(backupConnectionString))
                {
                    await sourceConnection.OpenAsync().ConfigureAwait(false);
                    await backupConnection.OpenAsync().ConfigureAwait(false); // Backup API hedef DB'yi kendi oluşturur

                    // Backup API'yi çağır
                    sourceConnection.BackupDatabase(backupConnection);

                    // Alternatif: Manuel sayfa sayfa kopyalama (daha fazla kontrol sağlar)
                    // const int pageSize = 4096; // Örnek sayfa boyutu
                    // sourceConnection.BackupDatabase(backupConnection, "main", "main", -1, null, pageSize);
                }


                _logger.LogInformation("SQLite backup completed successfully for '{DbPath}'.", dbPath);
                return new BackupResult(true, "Yedekleme başarıyla tamamlandı.", backupFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQLite backup failed for '{DbPath}'.", dbPath);
                return new BackupResult(false, $"SQLite yedekleme hatası: {ex.Message}", null);
            }
        }
    }
}
