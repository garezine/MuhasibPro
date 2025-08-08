using Microsoft.Data.SqlClient; // SQL Server için
using Microsoft.Extensions.Logging;
using Muhasebe.Data.Database.Helpers;
using Muhasebe.Data.Database.Interfaces.Operations;

namespace Muhasebe.Data.Database.Concreate.Providers.SqlServer
{
    // --- SQL Server Yedekleme ---
    public class SqlServerBackupOperations : IDatabaseBackupOperations
    {
        private readonly ILogger<SqlServerBackupOperations> _logger;

        public SqlServerBackupOperations(ILogger<SqlServerBackupOperations> logger)
        {
            _logger = logger;
        }

        public async Task<BackupResult> BackupDatabaseAsync(string connectionString, string dbName, string dbPath, string backupFilePath)
        {
            _logger.LogInformation("Starting SQL Server backup for database '{DbName}' to file '{BackupFilePath}'...", dbName, backupFilePath);

            try
            {
                // Yedekleme komutu. WITH FORMAT veya INIT, varolan yedek dosyasının üzerine yazılmasını sağlar.
                // Compression performansı artırabilir ama tüm SQL Server sürümlerinde desteklenmeyebilir.
                string backupCommand = $"BACKUP DATABASE [{dbName}] TO DISK = @backupFilePath WITH FORMAT, INIT, COMPRESSION;"; // COMPRESSION eklendi (opsiyonel)

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync().ConfigureAwait(false);

                using var command = new SqlCommand(backupCommand, connection);
                command.Parameters.AddWithValue("@backupFilePath", backupFilePath);
                // Komut zaman aşımını artırmak gerekebilir (büyük DB'ler için)
                command.CommandTimeout = 3600; // 1 saat örnek olarak

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                _logger.LogInformation("SQL Server backup completed successfully for database '{DbName}'.", dbName);
                return new BackupResult(true, "Yedekleme başarıyla tamamlandı.", backupFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQL Server backup failed for database '{DbName}'.", dbName);
                return new BackupResult(false, $"SQL Server yedekleme hatası: {ex.Message}");
            }
        }
    }
}
