using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantSqliteManager;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.TenantSqliteManager
{
    /// <summary>
    /// SADECE SQLite için connection string oluşturur
    /// </summary>
    public class TenantSQLiteConnectionStringFactory : ITenantSQLiteConnectionStringFactory
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly ILogger<TenantSQLiteConnectionStringFactory> _logger;

        // SQLite için optimal ayarlar
        private const SqliteOpenMode DefaultOpenMode = SqliteOpenMode.ReadWriteCreate;
        private const SqliteCacheMode DefaultCacheMode = SqliteCacheMode.Shared;
        private const int DefaultBusyTimeout = 5000;
        private const bool DefaultPooling = true;

        public TenantSQLiteConnectionStringFactory(
            IApplicationPaths applicationPaths,
            ILogger<TenantSQLiteConnectionStringFactory> logger)
        {
            _applicationPaths = applicationPaths ?? throw new ArgumentNullException(nameof(applicationPaths));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// SQLite için connection string oluşturur
        /// </summary>
        public string CreateConnectionString(string databaseName)
        {
            try
            {
                // ApplicationPaths zaten validasyon yapıyor
                var dbPath = _applicationPaths.GetTenantDatabaseFilePath(databaseName);

                var builder = new SqliteConnectionStringBuilder
                {
                    DataSource = dbPath,
                    Mode = DefaultOpenMode,
                    Cache = DefaultCacheMode,
                    Pooling = DefaultPooling,
                    ForeignKeys = true,
                    DefaultTimeout = DefaultBusyTimeout
                };
                var connectionString = builder.ToString();

                _logger.LogDebug(
                    "SQLite connection string oluşturuldu - Database: {DatabaseName}, Path: {DbPath}",
                    databaseName,
                    dbPath);

                return connectionString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection string oluşturulamadı: {DatabaseName}", databaseName);
                throw;
            }
        }

        /// <summary>
        /// SQLite bağlantısını test eder
        /// </summary>
        public async Task<bool> TestConnectionStringAsync(string databaseName, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionString = CreateConnectionString(databaseName);

                await using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync(cancellationToken);

                // Basit bir test sorgusu
                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                var result = await command.ExecuteScalarAsync(cancellationToken);

                var isSuccess = Convert.ToInt32(result) == 1;

                _logger.LogInformation(
                    "SQLite connection test {Result} for {DatabaseName}",
                    isSuccess ? "başarılı" : "başarısız",
                    databaseName);

                return isSuccess;
            }
            catch (SqliteException ex)
            {
                _logger.LogWarning(ex, "SQLite bağlantı testi başarısız: {DatabaseName}", databaseName);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Beklenmeyen hata: {DatabaseName}", databaseName);
                return false;
            }
        }
    }
}