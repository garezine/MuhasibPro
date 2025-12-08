using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Muhasib.Data.DataContext;
using Muhasib.Data.Managers.DatabaseManager.Concrete.Infrastructure;
using Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure;
using Muhasib.Data.Managers.DatabaseManager.Contracts.SistemDatabase;
using Muhasib.Data.Managers.DatabaseManager.Models;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.SistemDatabase
{
    public class SistemDatabaseManager : ISistemDatabaseManager
    {
        private readonly SistemDbContext _dbContext;
        private readonly ILogger<SistemDatabaseManager> _logger;        
        private readonly ISistemMigrationManager _migrationManager;
        private readonly ISistemBackupManager _backupManager;
        private readonly IApplicationPaths _applicationPaths;
        private const string databaseName = DatabaseConstants.SISTEM_DB_NAME;
        public SistemDatabaseManager(
            ILogger<SistemDatabaseManager> logger,
            ISistemMigrationManager migrationManager,
            ISistemBackupManager backupManager,
            IApplicationPaths applicationPaths,
            SistemDbContext dbContext)
        {
            _logger = logger;
            _migrationManager = migrationManager;
            _backupManager = backupManager;
            _applicationPaths = applicationPaths;
            _dbContext = dbContext;
        }
        
        public async Task<DatabaseHealthInfo> GetHealthStatusAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var healthInfo = new DatabaseHealthInfo();
                var testResult = await ValidateSistemDatabaseAsync(cancellationToken);
                if (testResult.IsValid)
                {                    
                    var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
                    var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
                    var appliedMigrations = await _dbContext.Database.GetAppliedMigrationsAsync(cancellationToken);
                    var backupFiles = await _backupManager.GetBackupsAsync();

                    var healtStatus = new DatabaseHealthInfo
                    {
                        CanConnect = canConnect,
                        PendingMigrationsCount = pendingMigrations.Count(),
                        AppliedMigrationsCount = appliedMigrations.Count(),
                        BackupFilesCount = backupFiles.Count,
                        LastBackupDate = _backupManager.GetLastBackupDate(),
                        DatabaseFileExists = _applicationPaths.SistemDatabaseFileExists(),
                        DatabaseName = databaseName,
                        DatabaseSize = _applicationPaths.GetSistemDatabaseSize(),
                        CheckTime = DateTime.Now
                    };
                    healthInfo = healtStatus;
                }
                return healthInfo;

            }
            catch (Exception ex)
            {
                return new DatabaseHealthInfo { HasError = true, ErrorMessage = ex.Message };
            }
        }
        public async Task<bool> InitializeDatabaseAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var databaseExists = await ValidateSistemDatabaseAsync(cancellationToken);

                if (!databaseExists.IsValid)
                    return false;


                _logger.LogInformation("Creating new database or update database: {DatabaseName}", databaseName);

                var intializeDatabase = await _migrationManager.InitializeDatabaseAsync(cancellationToken);

                _logger.LogInformation("Database created or updated successfully: {DatabaseName}", databaseName);
                return intializeDatabase;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create database: {DatabaseName}", databaseName);
                return false;
            }
        }

        public async Task<ConnectionTestResult> TestConnectionDetailedAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Testing connection for: {DatabaseName}", databaseName);

                // 1. Database varlığını kontrol et
                bool dbExists = _applicationPaths.SistemDatabaseFileExists();

                if (!dbExists)
                {
                    _logger.LogWarning("Tenant database not found: {DatabaseName}", databaseName);
                    return ConnectionTestResult.DatabaseNotFound;
                }

                // 2. Dosya boş mu kontrol et
                bool isValidSize = _applicationPaths.IsSistemDatabaseSizeValid();
                if (!isValidSize)
                {
                    _logger.LogWarning("Database file is empty: {DatabaseName}", databaseName);
                    return ConnectionTestResult.InvalidSchema;
                }
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
                    int tableCount = await _dbContext.Database
                        .SqlQueryRaw<int>(
                            "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name={0}",
                            "TenantDatabaseVersions")
                        .FirstOrDefaultAsync(cancellationToken);

                    if (tableCount == 0)
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

        public async Task<(bool IsValid, string Message)> ValidateSistemDatabaseAsync(CancellationToken cancellationToken = default)
        {
            var result = await TestConnectionDetailedAsync(cancellationToken);

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
