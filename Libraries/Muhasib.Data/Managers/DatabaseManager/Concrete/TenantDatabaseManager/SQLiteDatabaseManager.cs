using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasib.Data.DataContext.Factories;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantDatabaseManager;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantSqliteManager;
using Muhasib.Data.Managers.DatabaseManager.Models;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.TenantSqliteManager
{
    public class SQLiteDatabaseManager : ISQLiteDatabaseManager
    {
        private readonly ILogger<SQLiteDatabaseManager> _logger;
        private readonly IAppDbContextFactory _dbContextFactory;
        private readonly ITenantSQLiteMigrationManager _migrationManager;
        private readonly ITenantSQLiteBackupManager _backupManager;
        public SQLiteDatabaseManager(ILogger<SQLiteDatabaseManager> logger, IAppDbContextFactory dbContextFactory, ITenantSQLiteMigrationManager migrationManager, ITenantSQLiteBackupManager backupManager)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _migrationManager = migrationManager;
            _backupManager = backupManager;
        }
        public async Task<bool> CreateDatabaseAsync(string databaseName, CancellationToken cancellationToken = default)
        {
            try
            {
                bool databaseExists = _dbContextFactory.TenantDatabaseFileExists(databaseName);

                if (databaseExists)
                {
                    // Boyut geçerli mi?
                    bool isValidSize = _dbContextFactory.IsDatabaseSizeValid(databaseName);

                    if (isValidSize)
                    {
                        // ⭐ BOYUTU AL ve LOG'LA
                        long fileSizeBytes = _dbContextFactory.GetDatabaseSize(databaseName);

                        _logger.LogDebug("Database already exists: {DatabaseName} ({Size} bytes)",
                            databaseName, fileSizeBytes); // ⭐ fileSizeBytes (long)
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("Database file exists but is empty (0 bytes): {DatabaseName}",
                            databaseName);

                        // Boş dosyayı sil
                        string filePath = _dbContextFactory.GetTenantDatabaseFilePath(databaseName);
                        File.Delete(filePath);
                    }
                }

                _logger.LogInformation("Creating new database: {DatabaseName}", databaseName);

                await _migrationManager.InitializeDatabaseAsync(databaseName);

                _logger.LogInformation("Database created successfully: {DatabaseName}", databaseName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create database: {DatabaseName}", databaseName);
                return false;
            }
        }

        public async Task<bool> DatabaseExists(string databaseName, CancellationToken cancellationToken = default)
        => await _dbContextFactory.TestConnectionAsync(databaseName, cancellationToken);

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

        public async Task<DatabaseHealthInfo> GetHealthStatusAsync(string databaseName, CancellationToken cancellationToken = default)
        {
            try
            {
                var healthInfo = new DatabaseHealthInfo();
                var testResult =  await _dbContextFactory.TestConnectionAsync(databaseName, cancellationToken);
                if (testResult)
                {
                    using var context = _dbContextFactory.CreateContext(databaseName);
                    var canConnect = await context.Database.CanConnectAsync(cancellationToken);
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
                    var appliedMigrations = await context.Database.GetAppliedMigrationsAsync(cancellationToken);
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

            }
            catch (Exception ex)
            {
                return new DatabaseHealthInfo { HasError = true, ErrorMessage = ex.Message };
            }
        }
    }
}