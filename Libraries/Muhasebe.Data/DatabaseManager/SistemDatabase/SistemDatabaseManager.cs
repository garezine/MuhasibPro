using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Muhasebe.Data.DatabaseManager.Models;
using Muhasebe.Data.DataContext;
using Muhasebe.Data.Helper;

namespace Muhasebe.Data.DatabaseManager.SistemDatabase
{
    public class SistemDatabaseManager : ISistemDatabaseManager
    {
        private readonly AppSistemDbContext _context;
        private readonly ILogger<SistemDatabaseManager> _logger;
        private readonly string _databasePath;
        private readonly string _backupPath;

        public SistemDatabaseManager(
            AppSistemDbContext context,
            ILogger<SistemDatabaseManager> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;

            var dbDirectory = ConfigurationHelper.Instance.GetDatabasePath();
            _databasePath = Path.Combine(dbDirectory, "Sistem.db");
            _backupPath = Path.Combine(dbDirectory, "Backups", "Sistem");

            Directory.CreateDirectory(_backupPath);
        }

        public async Task<bool> InitializeDatabaseAsync()
        {
            try
            {
                _logger.LogInformation("Initializing system database...");

                // Migration'ları kontrol et
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();

                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("Found {Count} pending migrations", pendingMigrations.Count());

                    // Migration öncesi backup al
                    await CreateSafetyBackupAsync();

                    // Migration'ları uygula
                    await _context.Database.MigrateAsync();
                }

                // Validation
                var isValid = await ValidateDatabaseAsync();

                _logger.LogInformation("System database initialization completed: {Success}", isValid);
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System database initialization failed");
                return false;
            }
        }

        public async Task<bool> ValidateDatabaseAsync()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect) return false;

                // Temel tabloların kontrolü
                var hasUsers = await _context.Kullanicilar.AnyAsync();
                _logger.LogInformation("System validation - Can connect: {CanConnect}, Has users: {HasUsers}",
                    canConnect, hasUsers);

                return canConnect;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System database validation failed");
                return false;
            }
        }

        public async Task<bool> CreateManualBackupAsync()
        {
            try
            {
                var backupFileName = $"manual_sistem_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                var backupFilePath = Path.Combine(_backupPath, backupFileName);

                if (File.Exists(_databasePath))
                {
                    File.Copy(_databasePath, backupFilePath, true);
                    _logger.LogInformation("Manual system backup created: {BackupPath}", backupFilePath);

                    // Eski yedekleri temizle
                    await CleanOldBackupsAsync();

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manual system backup failed");
                return false;
            }
        }

        public async Task<bool> IsFirstRunAsync()
        {
            try
            {
                return !await _context.Kullanicilar.AnyAsync();
            }
            catch
            {
                return true; // Hata varsa first run kabul et
            }
        }



        public async Task<DatabaseHealthInfo> GetHealthInfoAsync()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();

                var backupFiles = Directory.Exists(_backupPath)
                    ? Directory.GetFiles(_backupPath, "*.db").Length
                    : 0;

                return new DatabaseHealthInfo
                {
                    CanConnect = canConnect,
                    PendingMigrationsCount = pendingMigrations.Count(),
                    AppliedMigrationsCount = appliedMigrations.Count(),
                    BackupFilesCount = backupFiles,
                    DatabaseSize = File.Exists(_databasePath) ? new FileInfo(_databasePath).Length : 0,
                    LastBackupDate = GetLastBackupDate()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get database health info");
                return new DatabaseHealthInfo { HasError = true, ErrorMessage = ex.Message };
            }
        }

        public Task<List<BackupFileInfo>> GetBackupHistoryAsync()
        {
            try
            {
                if (!Directory.Exists(_backupPath))
                    return Task.FromResult(new List<BackupFileInfo>());

                var backupFiles = Directory.GetFiles(_backupPath, "*.db")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .Take(20)
                    .Select(f => new BackupFileInfo
                    {
                        FileName = f.Name,
                        FilePath = f.FullName,
                        CreatedDate = f.CreationTime,
                        SizeBytes = f.Length,
                        SizeFormatted = FormatFileSize(f.Length)
                    })
                    .ToList();

                return Task.FromResult(backupFiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get backup history");
                return Task.FromResult(new List<BackupFileInfo>());
            }
        }

        private async Task CreateSafetyBackupAsync()
        {
            var backupFileName = $"safety_sistem_{DateTime.Now:yyyyMMdd_HHmmss}.db";
            var backupFilePath = Path.Combine(_backupPath, backupFileName);

            if (File.Exists(_databasePath))
            {
                File.Copy(_databasePath, backupFilePath, true);
                _logger.LogInformation("Safety backup created: {BackupPath}", backupFilePath);
            }
            await Task.CompletedTask;
        }

        private async Task CleanOldBackupsAsync(int keepCount = 10)
        {
            try
            {
                var backupFiles = Directory.GetFiles(_backupPath, "*.db")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .Skip(keepCount);

                foreach (var file in backupFiles)
                {
                    try
                    {
                        file.Delete();
                        _logger.LogInformation("Deleted old backup: {FileName}", file.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old backup: {FileName}", file.Name);
                    }
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clean old backups");
            }
        }

        private DateTime? GetLastBackupDate()
        {
            try
            {
                if (!Directory.Exists(_backupPath))
                    return null;

                var lastBackup = Directory.GetFiles(_backupPath, "*.db")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .FirstOrDefault();

                return lastBackup?.CreationTime;
            }
            catch
            {
                return null;
            }
        }
        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;

            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }

            return $"{number:n1}{suffixes[counter]}";
        }
    }
}
