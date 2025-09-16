using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasebe.Data.DataContext;

namespace Muhasebe.Data.Database.SistemDatabase
{
    public class SistemDatabaseManager : ISistemDatabaseManager
    {
        private readonly AppSistemDbContext _sistemContext;
        private readonly ILogger<SistemDatabaseManager> _logger;

        public SistemDatabaseManager(
            AppSistemDbContext sistemContext,
            ILogger<SistemDatabaseManager> logger)
        {
            _sistemContext = sistemContext;
            _logger = logger;
        }

        public async Task<bool> InitializeSistemDatabaseAsync()
        {
            try
            {
                _logger.LogInformation("Initializing sistem database...");

                // Veritabanı dizinini kontrol et ve oluştur
                await EnsureDatabaseDirectoryAsync();

                // Migration durumunu kontrol et
                var migrationStatus = await CheckMigrationStatusAsync();
                _logger.LogInformation($"Migration Status: {migrationStatus}");

                switch (migrationStatus)
                {
                    case MigrationStatus.NoDatabase:
                        _logger.LogInformation("Creating new database...");
                        await _sistemContext.Database.MigrateAsync();
                        break;

                    case MigrationStatus.PendingMigrations:
                        _logger.LogInformation("Applying pending migrations...");
                        await _sistemContext.Database.MigrateAsync();
                        break;

                    case MigrationStatus.UpToDate:
                        _logger.LogInformation("Database is up to date.");
                        break;

                    case MigrationStatus.Error:
                        _logger.LogError("Database initialization failed due to errors.");
                        return false;
                }

                // Bağlantıyı test et
                var canConnect = await _sistemContext.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogError("Cannot connect to database after initialization");
                    return false;
                }

                _logger.LogInformation("Sistem database initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sistem database initialization failed");
                return false;
            }
        }

        public async Task<MigrationStatus> CheckMigrationStatusAsync()
        {
            try
            {
                // Veritabanı var mı?
                var canConnect = await _sistemContext.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return MigrationStatus.NoDatabase;
                }

                // Pending migration'lar var mı?
                var pendingMigrations = await _sistemContext.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    _logger.LogInformation($"Pending migrations: {string.Join(", ", pendingMigrations)}");
                    return MigrationStatus.PendingMigrations;
                }

                return MigrationStatus.UpToDate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking migration status");
                return MigrationStatus.Error;
            }
        }

        public async Task<bool> ValidateSistemDatabaseAsync()
        {
            try
            {
                _logger.LogInformation("Validating sistem database...");

                var canConnect = await _sistemContext.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogError("Cannot connect to database");
                    return false;
                }

                // Temel tabloların varlığını kontrol et
                var hasKullanicilar = await _sistemContext.Kullanicilar.AnyAsync();
                _logger.LogInformation($"Users table exists and accessible: {hasKullanicilar}");

                _logger.LogInformation("Database validation completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database validation failed");
                return false;
            }
        }

        public async Task<string> GetCurrentSchemaVersionAsync()
        {
            try
            {
                var appliedMigrations = await _sistemContext.Database.GetAppliedMigrationsAsync();
                var lastMigration = appliedMigrations.LastOrDefault();
                return lastMigration ?? "No migrations applied";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get current schema version");
                return "Unknown";
            }
        }

        public async Task<List<string>> GetPendingMigrationsAsync()
        {
            try
            {
                var pendingMigrations = await _sistemContext.Database.GetPendingMigrationsAsync();
                return pendingMigrations.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get pending migrations");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetAppliedMigrationsAsync()
        {
            try
            {
                var appliedMigrations = await _sistemContext.Database.GetAppliedMigrationsAsync();
                return appliedMigrations.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get applied migrations");
                return new List<string>();
            }
        }

        public async Task<bool> BackupDatabaseAsync(string backupPath)
        {
            try
            {
                _logger.LogInformation($"Backing up database to: {backupPath}");

                var connectionString = _sistemContext.Database.GetConnectionString();
                if (connectionString?.Contains("Data Source=") == true)
                {
                    var dbPath = ExtractDbPathFromConnectionString(connectionString);

                    if (File.Exists(dbPath))
                    {
                        // Backup klasörünü oluştur
                        Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);

                        // Dosyayı kopyala
                        File.Copy(dbPath, backupPath, overwrite: true);

                        _logger.LogInformation("Database backup completed successfully");
                        
                        return await Task.FromResult(true);
                    }
                }

                _logger.LogError("Database file not found for backup");
                return await Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database backup failed");
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> RestoreDatabaseAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    _logger.LogError($"Backup file not found: {backupPath}");
                    return false;
                }

                _logger.LogInformation($"Restoring database from: {backupPath}");

                var connectionString = _sistemContext.Database.GetConnectionString();
                if (connectionString?.Contains("Data Source=") == true)
                {
                    var dbPath = ExtractDbPathFromConnectionString(connectionString);

                    // Mevcut veritabanını kapat
                    await _sistemContext.Database.CloseConnectionAsync();

                    // Backup'tan geri yükle
                    File.Copy(backupPath, dbPath, overwrite: true);

                    _logger.LogInformation("Database restore completed successfully");
                    return true;
                }

                _logger.LogError("Could not extract database path for restore");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database restore failed");
                return false;
            }
        }

        private async Task EnsureDatabaseDirectoryAsync()
        {
            var connectionString = _sistemContext.Database.GetConnectionString();
            if (connectionString?.Contains("Data Source=") == true)
            {
                var dbPath = ExtractDbPathFromConnectionString(connectionString);
                var directory = Path.GetDirectoryName(dbPath);

                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogInformation($"Created database directory: {directory}");
                }
            }
            await Task.CompletedTask;
        }

        private string ExtractDbPathFromConnectionString(string connectionString)
        {
            var startIndex = connectionString.IndexOf("Data Source=") + "Data Source=".Length;
            var endIndex = connectionString.IndexOf(";", startIndex);
            if (endIndex == -1) endIndex = connectionString.Length;

            return connectionString.Substring(startIndex, endIndex - startIndex).Trim();
        }
    }

    public enum MigrationStatus
    {
        NoDatabase,
        PendingMigrations,
        UpToDate,
        Error
    }
}
