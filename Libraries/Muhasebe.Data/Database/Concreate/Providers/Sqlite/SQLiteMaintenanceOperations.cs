using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Muhasebe.Data.Database.Interfaces.Operations;

namespace Muhasebe.Data.Database.Concreate.Providers.Sqlite
{
    // --- SQLite Bakım ---
    public class SQLiteMaintenanceOperations : IDatabaseMaintenanceOperations
    {
        private readonly ILogger<SQLiteMaintenanceOperations> _logger;

        public SQLiteMaintenanceOperations(ILogger<SQLiteMaintenanceOperations> logger)
        {
            _logger = logger;
        }

        private string GetConnectionString(string dbPath) => $"Data Source={dbPath}";

        public async Task CheckIntegrityAsync(string connectionString, string dbName, string dbPath)
        {
            _logger.LogInformation("Starting SQLite integrity check for database '{DbPath}'...", dbPath);
            string sql = "PRAGMA integrity_check;";
            try
            {
                using var connection = new SqliteConnection(GetConnectionString(dbPath));
                await connection.OpenAsync().ConfigureAwait(false);
                using var command = new SqliteCommand(sql, connection);
                _logger.LogDebug("Executing: {Sql}", sql);
                var result = (await command.ExecuteScalarAsync().ConfigureAwait(false))?.ToString();

                if ("ok".Equals(result, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("SQLite integrity check passed for '{DbPath}'.", dbPath);
                }
                else
                {
                    _logger.LogError("SQLite integrity check failed for '{DbPath}'. Result: {Result}", dbPath, result);
                    // Hata fırlatmak yerine loglamak yeterli olabilir, servis karar verir.
                    throw new Exception($"SQLite integrity check failed. Result: {result}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQLite integrity check encountered an error for '{DbPath}'.", dbPath);
                throw;
            }
        }

        public async Task ReindexTablesAsync(string connectionString, string dbName, string dbPath)
        {
            _logger.LogInformation("Starting SQLite reindex operation for database '{DbPath}'...", dbPath);
            string sql = "REINDEX;";
            try
            {
                using var connection = new SqliteConnection(GetConnectionString(dbPath));
                await connection.OpenAsync().ConfigureAwait(false);
                using var command = new SqliteCommand(sql, connection) { CommandTimeout = 1800 }; // Reindex uzun sürebilir
                _logger.LogDebug("Executing: {Sql}", sql);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                _logger.LogInformation("SQLite reindex operation completed for '{DbPath}'.", dbPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQLite reindex operation failed for '{DbPath}'.", dbPath);
                throw;
            }
        }

        public async Task ShrinkDatabaseAsync(string connectionString, string dbName, string dbPath)
        {
            _logger.LogInformation("Starting SQLite shrink (VACUUM) operation for database '{DbPath}'...", dbPath);
            string sql = "VACUUM;";
            try
            {
                using var connection = new SqliteConnection(GetConnectionString(dbPath));
                await connection.OpenAsync().ConfigureAwait(false);
                using var command = new SqliteCommand(sql, connection) { CommandTimeout = 3600 }; // VACUUM çok uzun sürebilir
                _logger.LogDebug("Executing: {Sql}", sql);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                _logger.LogInformation("SQLite shrink (VACUUM) operation completed for '{DbPath}'.", dbPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQLite shrink (VACUUM) operation failed for '{DbPath}'.", dbPath);
                throw;
            }
        }
    }
}
