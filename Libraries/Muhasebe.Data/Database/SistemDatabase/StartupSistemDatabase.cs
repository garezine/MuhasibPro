using Microsoft.Extensions.Logging;
using Muhasebe.Data.Helper;

namespace Muhasebe.Data.Database.SistemDatabase
{
    public class StartupSistemDatabase
    {
        private readonly ISistemDatabaseManager _sistemDatabaseManager;
        private readonly ILogger<StartupSistemDatabase> _logger;

        public StartupSistemDatabase(ISistemDatabaseManager sistemDatabaseManager, ILogger<StartupSistemDatabase> logger)
        {
            _sistemDatabaseManager = sistemDatabaseManager;
            _logger = logger;
        }

        /// <summary>
        /// Sistem veritabanını kontrol eder, backup alır ve gerekirse günceller
        /// </summary>
        public async Task<StartupResult> InitializeSistemDatabaseAsync()
        {
            var result = new StartupResult();

            try
            {
                _logger.LogInformation("Starting database initialization process...");

                // 1. Veritabanı durumunu kontrol et
                _logger.LogInformation("Checking database migration status...");
                var status = await _sistemDatabaseManager.CheckMigrationStatusAsync();
                result.MigrationStatus = status;

                _logger.LogInformation($"Database migration status: {status}");

                // 2. Eğer veritabanı varsa backup al
                if(status != MigrationStatus.NoDatabase)
                {
                    _logger.LogInformation("Taking database backup...");
                    var backupResult = await CreateBackupAsync();
                    result.BackupPath = backupResult.BackupPath;
                    result.BackupSuccessful = backupResult.Success;

                    if(!backupResult.Success)
                    {
                        _logger.LogWarning("Backup failed, but continuing with database initialization...");
                    } else
                    {
                        _logger.LogInformation($"Backup created successfully: {backupResult.BackupPath}");
                    }
                }

                // 3. Migration durumuna göre işlem yap
                switch(status)
                {
                    case MigrationStatus.NoDatabase:
                        _logger.LogInformation("No database found. Creating new database...");
                        result.Action = "Creating new database";
                        await _sistemDatabaseManager.InitializeSistemDatabaseAsync();
                        break;

                    case MigrationStatus.PendingMigrations:
                        _logger.LogInformation("Pending migrations found. Applying updates...");
                        result.Action = "Applying database updates";

                        // Pending migration'ları logla
                        var pendingMigrations = await _sistemDatabaseManager.GetPendingMigrationsAsync();
                        _logger.LogInformation($"Pending migrations: {string.Join(", ", pendingMigrations)}");
                        result.PendingMigrations = pendingMigrations;

                        await _sistemDatabaseManager.InitializeSistemDatabaseAsync();
                        break;

                    case MigrationStatus.UpToDate:
                        _logger.LogInformation("Database is up to date. No action needed.");
                        result.Action = "Database up to date";
                        break;

                    case MigrationStatus.Error:
                        _logger.LogError("Database status check failed.");
                        result.Action = "Error checking database";
                        result.Success = false;
                        result.ErrorMessage = "Database status check failed";
                        return result;
                }

                // 4. Final validation
                _logger.LogInformation("Validating database...");
                var validationResult = await _sistemDatabaseManager.ValidateSistemDatabaseAsync();

                if(!validationResult)
                {
                    _logger.LogError("Database validation failed after initialization");
                    result.Success = false;
                    result.ErrorMessage = "Database validation failed";
                    return result;
                }

                // 5. Schema version'ı logla
                var currentVersion = await _sistemDatabaseManager.GetCurrentSchemaVersionAsync();
                _logger.LogInformation($"Current database schema version: {currentVersion}");
                result.CurrentSchemaVersion = currentVersion;

                _logger.LogInformation("Database initialization completed successfully");
                result.Success = true;
                return result;
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Database initialization failed: {Message}", ex.Message);
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.Exception = ex;
                return result;
            }
        }

        /// <summary>
        /// Veritabanı backup'ı oluşturur
        /// </summary>
        private async Task<BackupResult> CreateBackupAsync()
        {
            try
            {
                // Backup klasörünü oluştur
                var backupDir = Path.Combine(ConfigurationHelper.Instance.GetProjectPath(), "Backups");

                if(!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                // Backup dosya adı
                var backupFileName = $"sistem_DB_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                var backupPath = Path.Combine(backupDir, backupFileName);

                // Backup al
                var success = await _sistemDatabaseManager.BackupDatabaseAsync(backupPath);

                if(success)
                {
                    // Eski backup'ları temizle (son 5'ini tut)
                    await CleanOldBackupsAsync(backupDir, keepCount: 5);
                }

                return new BackupResult { Success = success, BackupPath = success ? backupPath : null };
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Backup creation failed");
                return new BackupResult { Success = false, BackupPath = null };
            }
        }

        /// <summary>
        /// Eski backup dosyalarını temizler
        /// </summary>
        private async Task CleanOldBackupsAsync(string backupDir, int keepCount)
        {
            try
            {
                var backupFiles = Directory.GetFiles(backupDir, "sistem_backup_*.db")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .Skip(keepCount);

                foreach(var file in backupFiles)
                {
                    try
                    {
                        file.Delete();
                        _logger.LogInformation($"Deleted old backup: {file.Name}");
                    } catch(Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to delete old backup: {file.Name}");
                    }
                }
                await Task.CompletedTask;
            } catch(Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clean old backups");
            }
        }

        /// <summary>
        /// Acil durum database restore'u
        /// </summary>
        public async Task<bool> RestoreFromBackupAsync(string backupPath)
        {
            try
            {
                _logger.LogInformation($"Restoring database from backup: {backupPath}");

                var success = await _sistemDatabaseManager.RestoreDatabaseAsync(backupPath);

                if(success)
                {
                    _logger.LogInformation("Database restore completed successfully");

                    // Restore sonrası validation
                    var validationResult = await _sistemDatabaseManager.ValidateSistemDatabaseAsync();
                    if(!validationResult)
                    {
                        _logger.LogWarning("Database validation failed after restore");
                        return false;
                    }
                } else
                {
                    _logger.LogError("Database restore failed");
                }

                return success;
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Database restore failed");
                return false;
            }
        }
    }

    /// <summary>
    /// Database initialization sonucu
    /// </summary>
    public class StartupResult
    {
        public bool Success { get; set; } = true;

        public string ErrorMessage { get; set; }

        public Exception Exception { get; set; }

        public MigrationStatus MigrationStatus { get; set; }

        public string Action { get; set; }

        public bool BackupSuccessful { get; set; }

        public string BackupPath { get; set; }

        public List<string> PendingMigrations { get; set; } = new();

        public string CurrentSchemaVersion { get; set; }
    }

    /// <summary>
    /// Backup işlemi sonucu
    /// </summary>
    public class BackupResult
    {
        public bool Success { get; set; }

        public string BackupPath { get; set; }
    }
}
