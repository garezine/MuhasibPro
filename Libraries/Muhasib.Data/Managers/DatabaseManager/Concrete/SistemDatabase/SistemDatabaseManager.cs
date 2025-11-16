using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasib.Data.DataContext;
using Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure;
using Muhasib.Data.Managers.DatabaseManager.Contracts.SistemDatabase;
using Muhasib.Data.Managers.DatabaseManager.Models;
using Muhasib.Domain.Entities.SistemEntity;
using System.Diagnostics;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.SistemDatabase
{
    public static class DatabaseConstants
    {
        public const string SistemDbAdi = "Sistem.db";
        public const string DatabaseKlasorAdi = "Databases";
        public const string YedekKlasorAdi = "Backups";
        public const string GeciciKlasorAdi = "Temp";
    }
    public class SistemDatabaseManager : ISistemDatabaseManager
    {
        private readonly SistemDbContext _context;
        private readonly ILogger<SistemDatabaseManager> _logger;
        private readonly IApplicationPaths _applicationPaths;
        private readonly string _databasePath;
        private readonly string _backupPath;

        public SistemDatabaseManager(
            SistemDbContext context,
            ILogger<SistemDatabaseManager> logger,
            IApplicationPaths applicationPaths)
        {
            _context = context;
            _logger = logger;
            _applicationPaths = applicationPaths;

            _databasePath = Path.Combine(_applicationPaths.GetSistemDbPath());
            _backupPath = Path.Combine(_applicationPaths.GetBackupPath(), "Sistem");

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
                    Debug.Write("Sistem Veritabanı yedeklendi");

                    // Migration'ları uygula
                    await _context.Database.MigrateAsync();
                    Debug.Write("Sistem Veritabanı için yeni migrationlar uygulandı");
                }
                else
                {
                    await _context.Database.EnsureCreatedAsync();
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
                if (!canConnect)
                    return false;

                // Temel tabloların kontrolü
                var hasUsers = await _context.Kullanicilar.AnyAsync();
                _logger.LogInformation(
                    "System validation - Can connect: {CanConnect}, Has users: {HasUsers}",
                    canConnect,
                    hasUsers);

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
                // 1. Database file exists mi?
                if (!File.Exists(_databasePath))
                    return true;

                // 2. Can connect mi?
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                    return true;

                // 3. Tüm critical tablolar var mı ve boş mu?
                var hasUsers = await _context.Kullanicilar.AnyAsync();
                var hasVersion = await _context.SistemDbVersiyonlar.AnyAsync();

                // Tablolar var ama boşsa first-run (seed gerekiyor)
                return !hasUsers || !hasVersion;
            }
            catch
            {
                return true; // Hata durumunda first-run kabul et
            }
        }


        public async Task<DatabaseHealthInfo> GetHealthInfoAsync()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();

                var backupFiles = Directory.Exists(_backupPath) ? Directory.GetFiles(_backupPath, "*.db").Length : 0;

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
                    .Select(
                        f => new BackupFileInfo
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

        public async Task<bool> CheckSistemDatabaseConnectionAsync()
        {
            var dbPath = Path.Combine(_applicationPaths.GetSistemDbPath());
            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                try
                {
                    await connection.OpenAsync(); // Bağlantıyı aç

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' LIMIT 1";
                        var result = await command.ExecuteScalarAsync();
                        return result != null;
                    }
                }
                catch
                {
                    return false;
                }
                // using bloğu çıkışında connection otomatik kapanır
            }
        }

        public async Task<AppVersiyon> GetCurrentAppVersionAsync()
        {
            return await _context.AppVersiyonlar
                .OrderByDescending(v => v.UygulamaSonGuncellemeTarihi)
                .AsNoTracking() // Performance için
                .FirstOrDefaultAsync();
        }

        public async Task<SistemDbVersiyon> GetCurrentSistemDbVersionAsync()
        {
            return await _context.SistemDbVersiyonlar
                .OrderByDescending(v => v.SistemDBSonGuncellemeTarihi)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }
}
