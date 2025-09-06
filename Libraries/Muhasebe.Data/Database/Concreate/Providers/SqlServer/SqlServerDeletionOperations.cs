using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Muhasebe.Data.Database.Helpers;
using Muhasebe.Data.Database.Interfaces.Operations;

namespace Muhasebe.Data.Database.Concreate.Providers.SqlServer
{
    // --- SQL Server Silme ---
    public class SqlServerDeletionOperations : IDatabaseDeletionOperations
    {
        private readonly ILogger<SqlServerDeletionOperations> _logger;
        // Opsiyonel: private readonly IDatabaseDirectoryManager _directoryManager; // Fiziksel dosya/dizin temizliği için

        public SqlServerDeletionOperations(ILogger<SqlServerDeletionOperations> logger) { _logger = logger; }

        public async Task<DeletionResult> DeleteDatabaseAsync(
            string connectionString,
            string dbName,
            string dbDirectory,
            string dbPath)
        {
            _logger.LogWarning("Attempting to delete SQL Server database '{DbName}'...", dbName);

            // DROP DATABASE komutu master veritabanından çalıştırılmalı.
            string masterConnectionString = GetMasterConnectionString(connectionString);
            if (string.IsNullOrEmpty(masterConnectionString))
            {
                return new DeletionResult(false, "Master veritabanı bağlantı cümlesi oluşturulamadı.");
            }

            try
            {
                using var connection = new SqlConnection(masterConnectionString);
                await connection.OpenAsync().ConfigureAwait(false);

                // 1. Aktif bağlantıları kes (SINGLE_USER modu ile)
                string alterDbSql = $"IF DB_ID('{dbName}') IS NOT NULL ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
                using (var alterCommand = new SqlCommand(alterDbSql, connection))
                {
                    _logger.LogDebug("Executing: {Sql}", alterDbSql);
                    await alterCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    _logger.LogInformation("Set database '{DbName}' to SINGLE_USER mode.", dbName);
                }

                // 2. Veritabanını sil (DROP)
                string dropDbSql = $"IF DB_ID('{dbName}') IS NOT NULL DROP DATABASE [{dbName}];";
                using (var dropCommand = new SqlCommand(dropDbSql, connection))
                {
                    _logger.LogDebug("Executing: {Sql}", dropDbSql);
                    await dropCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                _logger.LogInformation("SQL Server database '{DbName}' deleted successfully.", dbName);

                // 3. Opsiyonel: Fiziksel dosyaları ve dizini temizle
                // SQL Server genellikle dosyaları kendi siler, ancak emin olmak veya boş dizini
                // silmek için IDatabaseDirectoryManager kullanılabilir.
                // try
                // {
                //      if (Directory.Exists(dbDirectory))
                //      {
                //           Directory.Delete(dbDirectory, true); // DİKKAT: Tüm içeriği siler!
                //          _logger.LogInformation("Deleted database directory: {Directory}", dbDirectory);
                //      }
                // } catch (Exception dirEx) { _logger.LogWarning(dirEx, "Could not delete database directory: {Directory}", dbDirectory); }


                return new DeletionResult(true, "Veritabanı başarıyla silindi.", dbName, dbDirectory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQL Server database deletion failed for '{DbName}'.", dbName);
                // Hata durumunda SINGLE_USER modunu geri almak gerekebilir, ancak DB drop olduysa zaten anlamsız.
                return new DeletionResult(false, $"SQL Server silme hatası: {ex.Message}", dbName, dbDirectory);
            }
        }

        private string GetMasterConnectionString(string connectionString)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString)
                {
                    InitialCatalog = "master" // Veritabanı adını master olarak değiştir
                };
                return builder.ConnectionString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build master connection string.");
                return null;
            }
        }
    }
}

