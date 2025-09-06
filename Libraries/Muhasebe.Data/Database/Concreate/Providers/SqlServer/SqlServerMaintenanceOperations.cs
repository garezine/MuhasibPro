using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Muhasebe.Data.Database.Interfaces.Operations;

namespace Muhasebe.Data.Database.Concreate.Providers.SqlServer
{
    // --- SQL Server Bakım ---
    public class SqlServerMaintenanceOperations : IDatabaseMaintenanceOperations
    {
        private readonly ILogger<SqlServerMaintenanceOperations> _logger;

        public SqlServerMaintenanceOperations(ILogger<SqlServerMaintenanceOperations> logger)
        {
            _logger = logger;
        }

        public async Task CheckIntegrityAsync(string connectionString, string dbName, string dbPath)
        {
            _logger.LogInformation("Starting SQL Server integrity check for database '{DbName}'...", dbName);
            string sql = $"DBCC CHECKDB ([{dbName}]) WITH NO_INFOMSGS;";
            try
            {
                using var connection = new SqlConnection(connectionString); // Target DB'ye bağlan
                await connection.OpenAsync().ConfigureAwait(false);
                using var command = new SqlCommand(sql, connection) { CommandTimeout = 1800 }; // Zaman aşımı artırıldı
                _logger.LogDebug("Executing: {Sql}", sql);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                _logger.LogInformation("SQL Server integrity check completed successfully for '{DbName}'.", dbName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQL Server integrity check failed for '{DbName}'.", dbName);
                throw; // Hata yukarı fırlatılmalı
            }
        }

        public async Task ReindexTablesAsync(string connectionString, string dbName, string dbPath)
        {
            _logger.LogInformation("Starting SQL Server table reindexing for database '{DbName}'...", dbName);
            // Bu script tüm tablolar ve indeksler üzerinde döner. Büyük DB'lerde uzun sürebilir.
            // Daha sofistike çözümler sadece parçalanmış (fragmented) indeksleri hedefleyebilir.
            string sql = @"
                SET NOCOUNT ON;
                DECLARE @tableName NVARCHAR(255);
                DECLARE @sql NVARCHAR(MAX);
                DECLARE tableCursor CURSOR FOR
                SELECT TABLE_SCHEMA + '.' + TABLE_NAME
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_TYPE = 'BASE TABLE';

                OPEN tableCursor;
                FETCH NEXT FROM tableCursor INTO @tableName;
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    SET @sql = 'ALTER INDEX ALL ON ' + @tableName + ' REBUILD;';
                    PRINT 'Rebuilding indexes on table: ' + @tableName;
                    BEGIN TRY
                       EXEC sp_executesql @sql;
                    END TRY
                    BEGIN CATCH
                       PRINT 'Error rebuilding indexes on table ' + @tableName + ': ' + ERROR_MESSAGE();
                       -- İsteğe bağlı: Hata durumunda devam et veya dur
                    END CATCH
                    FETCH NEXT FROM tableCursor INTO @tableName;
                END;
                CLOSE tableCursor;
                DEALLOCATE tableCursor;";

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync().ConfigureAwait(false);
                using var command = new SqlCommand(sql, connection) { CommandTimeout = 7200 }; // Zaman aşımı çok daha uzun olabilir
                _logger.LogDebug("Executing reindex script for '{DbName}'...", dbName);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                _logger.LogInformation("SQL Server table reindexing completed for '{DbName}'.", dbName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQL Server table reindexing failed for '{DbName}'.", dbName);
                throw;
            }
        }

        public async Task ShrinkDatabaseAsync(string connectionString, string dbName, string dbPath)
        {
            _logger.LogInformation("Starting SQL Server shrink operation for database '{DbName}'...", dbName);
            // Uyarı: Shrink işlemi genellikle önerilmez (performans sorunları, fragmentation).
            // Sadece çok özel durumlarda ve dikkatli kullanılmalıdır.
            string sql = $"DBCC SHRINKDATABASE ([{dbName}]);";
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync().ConfigureAwait(false);
                using var command = new SqlCommand(sql, connection) { CommandTimeout = 3600 };
                _logger.LogDebug("Executing: {Sql}", sql);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                // İsteğe bağlı olarak log dosyasını da shrink etmek için:
                // DBCC SHRINKFILE (log_file_name, target_size_in_mb);
                _logger.LogInformation("SQL Server shrink operation completed for '{DbName}'.", dbName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQL Server shrink operation failed for '{DbName}'.", dbName);
                throw;
            }
        }
    }
}