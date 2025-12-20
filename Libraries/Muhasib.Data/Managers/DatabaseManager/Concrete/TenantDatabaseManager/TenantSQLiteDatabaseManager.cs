using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasib.Data.DataContext.Factories;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantDatabaseManager;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantSqliteManager;
using Muhasib.Data.Managers.DatabaseManager.Models;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.TenantSqliteManager
{
    public class TenantSQLiteDatabaseManager : ITenantSQLiteDatabaseManager
    {
        private readonly ILogger<TenantSQLiteDatabaseManager> _logger;
        private readonly IAppDbContextFactory _dbContextFactory;
        private readonly ITenantSQLiteMigrationManager _migrationManager;
        private readonly ITenantSQLiteBackupManager _backupManager;
        

        public TenantSQLiteDatabaseManager(
            ILogger<TenantSQLiteDatabaseManager> logger,
            IAppDbContextFactory dbContextFactory,
            ITenantSQLiteMigrationManager migrationManager,
            ITenantSQLiteBackupManager backupManager
            )
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _migrationManager = migrationManager;
            _backupManager = backupManager;
            
        }

        public async Task<DatabaseHealthInfo> GetHealthStatusAsync(
            string databaseName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var healthInfo = new DatabaseHealthInfo();
                var testResult = await ValidateTenantDatabaseAsync(databaseName, cancellationToken);
                if(testResult.IsValid)
                {
                    using var _dbcontext = _dbContextFactory.CreateContext(databaseName);
                    var canConnect = await _dbcontext.Database.CanConnectAsync(cancellationToken);
                    var pendingMigrations = await _dbcontext.Database.GetPendingMigrationsAsync(cancellationToken);
                    var appliedMigrations = await _dbcontext.Database.GetAppliedMigrationsAsync(cancellationToken);
                    var backupFiles = await _backupManager.GetBackupsAsync(databaseName);

                    var healtStatus = new DatabaseHealthInfo
                    {
                        CanConnect = canConnect,
                        PendingMigrationsCount = pendingMigrations.Count(),
                        AppliedMigrationsCount = appliedMigrations.Count(),
                        BackupFilesCount = backupFiles.Count,
                        LastBackupDate = _backupManager.GetLastBackupDate(databaseName),
                        DatabaseFileExists = _dbContextFactory.TenantDatabaseFileExists(databaseName),
                        DatabaseName = databaseName,
                        DatabaseSize = _dbContextFactory.GetDatabaseSize(databaseName),
                        CheckTime = DateTime.Now
                    };
                    healthInfo = healtStatus;
                }
                return healthInfo;
            } catch(Exception ex)
            {
                return new DatabaseHealthInfo { HasError = true, ErrorMessage = ex.Message };
            }
        }
        private async Task<ConnectionTestResult> TestConnectionDetailedAsync(string databaseName,CancellationToken cancellationToken = default)
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
                bool isValidSize = _dbContextFactory.IsTenantDatabaseSizeValid(databaseName);
                if (!isValidSize)
                {
                    _logger.LogWarning("Database file is empty: {DatabaseName}", databaseName);
                    return ConnectionTestResult.InvalidSchema;
                }
                using var _dbContext = _dbContextFactory.CreateContext(databaseName);
                bool canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
                if (!canConnect)
                {
                    _logger.LogWarning("Connection failed: {DatabaseName}", databaseName);
                    return ConnectionTestResult.ConnectionFailed;
                }

                // 4. Schema validation (Doğru yöntemle)
                try
                {
                    // ⭐ DÜZELTİLDİ: ExecuteSqlRawAsync → SqlQueryRaw
                    var result = await _dbContext.Database
                        .SqlQueryRaw<TableExistsResult>(
                            @"SELECT COUNT(*) as TableCount 
          FROM sqlite_master 
          WHERE type='table' AND name = @p0",
                            "AppLogs")
                        .FirstOrDefaultAsync(cancellationToken);

                    if (result?.TableCount == 0)
                    {
                        _logger.LogWarning("Applogs tablosu bulunamadı: {DatabaseName}", databaseName);
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
        public async Task<bool> CreateOrUpdateDatabaseAsync(string databaseName, CancellationToken cancellationToken = default)
        {
            try
            {
                if(_dbContextFactory.TenantDatabaseFileExists(databaseName))
                {
                    _logger.LogWarning("Database already exists: {DatabaseName}", databaseName);
                    return false;
                }
                _logger.LogInformation("Creating new database: {DatabaseName}", databaseName);
                var migrationResult = await _migrationManager.InitializingDatabaseAsync(
                    databaseName,
                    cancellationToken);

                _logger.LogInformation("Database created successfully: {DatabaseName}", databaseName);
                return migrationResult;
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to create database: {DatabaseName}", databaseName);
                return false;
            }
        }
        public async Task<bool> DeleteDatabaseAsync(string databaseName, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(databaseName))
            {
                _logger.LogWarning("Delete için boş database adı");
                return false;
            }

            var databaseExists = _dbContextFactory.TenantDatabaseFileExists(databaseName);

            if(!databaseExists)
            {
                _logger.LogDebug("Database file not found: {DatabaseName}", databaseName);
                return true;
            }

            // RETRY mekanizması (3 deneme)
            for(int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    SqliteConnection.ClearAllPools();

                    // Attempt sayısına göre bekleme süresi
                    await Task.Delay(100 * attempt, cancellationToken);

                    File.Delete(_dbContextFactory.GetTenantDatabaseFilePath(databaseName));

                    _logger.LogInformation(
                        "Database deleted: {DatabaseName} (attempt {Attempt})",
                        databaseName,
                        attempt);
                    await _backupManager.CleanupSqliteWalFilesAsync(databaseName);
                    return true;
                } catch(IOException ex) when (attempt < 3)
                {
                    _logger.LogWarning(ex, "Delete attempt {Attempt} failed for {DatabaseName}", attempt, databaseName);
                    continue;
                } catch(Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete database: {DatabaseName}", databaseName);
                    return false;
                }
            }

            return false;
        }
        public async Task<(bool IsValid, string Message)> ValidateTenantDatabaseAsync(string databaseName, CancellationToken cancellationToken = default)
        {
            var result = await TestConnectionDetailedAsync(databaseName,cancellationToken);

            return result switch
            {
                ConnectionTestResult.Success => (true, $"{databaseName} Tenant bağlantısı başarılı"),
                ConnectionTestResult.DatabaseNotFound => (false, "Tenant veritabanı bulunamadı"),
                ConnectionTestResult.ConnectionFailed => (false, "Tenant veritabanına bağlanılamıyor"),
                ConnectionTestResult.InvalidSchema => (false, "Tenant veritabanı geçersiz şemaya sahip"),
                ConnectionTestResult.UnknownError => (false, "Bilinmeyen bağlantı hatası"),
                _ => (false, $"Bağlantı hatası: {result}")
            };
        }
    }
}