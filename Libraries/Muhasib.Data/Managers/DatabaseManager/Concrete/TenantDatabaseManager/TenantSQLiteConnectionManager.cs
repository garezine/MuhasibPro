using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasib.Data.DataContext.Factories;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantDatabaseManager;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantSqliteManager;
using Muhasib.Data.Managers.DatabaseManager.Models;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.TenantDatabaseManager
{
    public class TenantSQLiteConnectionManager : ITenantSQLiteConnectionManager
    {
        private readonly ITenantSQLiteConnectionStringFactory _connectionStringFactory;
        private readonly IAppDbContextFactory _dbContextFactory;
        private readonly ILogger<TenantSQLiteConnectionManager> _logger;

        public TenantSQLiteConnectionManager(
            ITenantSQLiteConnectionStringFactory connectionStringFactory,
            IAppDbContextFactory dbContextFactory,
            ILogger<TenantSQLiteConnectionManager> logger)
        {
            _connectionStringFactory = connectionStringFactory;
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public Task<string> GetConnectionStringAsync(string databaseName)
        {
            return Task.FromResult(_connectionStringFactory.CreateConnectionString(databaseName));
        }

        public Task<string> GetConnectionStringInfoAsync(string databaseName)
        {
            try
            {
                using var dbContext = _dbContextFactory.CreateContext(databaseName);
                return Task.FromResult(dbContext.Database.GetDbConnection().ConnectionString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bağlantı dizesi getirilemedi: {DatabaseName}", databaseName);
                return Task.FromResult(string.Empty);
            }
        }

        public async Task<ConnectionTestResult> TestConnectionDetailedAsync(string databaseName)
        {
            try
            {
                _logger.LogDebug("Testing connection for: {DatabaseName}", databaseName);

                // 1. Database varlığını kontrol et
                bool dbExists = _dbContextFactory.TenantDatabaseFileExists(databaseName);

                if (!dbExists)
                {
                    _logger.LogWarning("Tenant database not found: {DatabaseName}", databaseName);
                    return ConnectionTestResult.DatabaseNotFound;
                }

                // 2. Dosya boş mu kontrol et
                bool isValidSize = _dbContextFactory.IsDatabaseSizeValid(databaseName);
                if (!isValidSize)
                {
                    _logger.LogWarning("Database file is empty: {DatabaseName}", databaseName);
                    return ConnectionTestResult.InvalidSchema;
                }

                // 3. Bağlantı testi
                using var tenantDbContext = _dbContextFactory.CreateContext(databaseName);

                bool canConnect = await tenantDbContext.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogWarning("Connection failed: {DatabaseName}", databaseName);
                    return ConnectionTestResult.ConnectionFailed;
                }

                // 4. Schema validation (Doğru yöntemle)
                try
                {
                    var result = await tenantDbContext.Database
                        .SqlQueryRaw<TableExistsResult>(
                            @"SELECT COUNT(*) as TableCount 
          FROM sqlite_master 
          WHERE type='table' AND name = @p0",
                            "TenantDatabaseVersions")
                        .FirstOrDefaultAsync();

                    if (result?.TableCount == 0)
                    {
                        _logger.LogWarning("TenantDatabaseVersions tablosu bulunamadı: {DatabaseName}", databaseName);
                        return ConnectionTestResult.InvalidSchema;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Schema validation başarısız: {DatabaseName}", databaseName);
                    return ConnectionTestResult.InvalidSchema;
                }

                _logger.LogInformation("Connection test successful: {DatabaseName}", databaseName);
                return ConnectionTestResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection test error: {DatabaseName}", databaseName);
                return ConnectionTestResult.UnknownError;
            }
        }

        public async Task<(bool IsValid, string Message)> ValidateTenantAsync(string databaseName)
        {
            var result = await TestConnectionDetailedAsync(databaseName);

            return result switch
            {
                ConnectionTestResult.Success => (true, "Tenant bağlantısı başarılı"),
                ConnectionTestResult.DatabaseNotFound => (false, "Tenant veritabanı bulunamadı"),
                ConnectionTestResult.ConnectionFailed => (false, "Tenant veritabanına bağlanılamıyor"),
                ConnectionTestResult.InvalidSchema => (false, "Tenant veritabanı geçersiz şemaya sahip"),
                ConnectionTestResult.UnknownError => (false, "Bilinmeyen bağlantı hatası"),
                _ => (false, $"Bağlantı hatası: {result}")
            };
        }
    }
}