using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantSqliteManager;
using Muhasib.Data.Managers.DatabaseManager.Models;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.TenantSqliteManager
{
    public class TenantSQLiteBackupManager : ITenantSQLiteBackupManager
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly ILogger<TenantSQLiteBackupManager> _logger;

        public TenantSQLiteBackupManager(
            IApplicationPaths applicationPaths,
            ILogger<TenantSQLiteBackupManager> logger = null)
        {
            _applicationPaths = applicationPaths;
            _logger = logger;
        }

        /// <summary>
        /// Backup oluşturur
        /// </summary>
        public async Task<bool> CreateBackupAsync(string databaseName, CancellationToken cancellationToken = default)
        {
            try
            {
                var sourcePath = _applicationPaths.GetTenantDatabaseFilePath(databaseName);
                if (!File.Exists(sourcePath))
                {
                    _logger?.LogWarning("Backup için kaynak dosya bulunamadı: {DatabaseName}", databaseName);
                    return false;
                }

                var backupDir = _applicationPaths.GetTenantBackupFolderPath();
                var backupFileName = GenerateBackupFileName(databaseName);
                var backupPath = Path.Combine(backupDir, backupFileName);
                
                await SafeFileCopyAsync(sourcePath, backupPath, cancellationToken);

                _logger?.LogInformation("Backup oluşturuldu: {DatabaseName} -> {BackupPath}", databaseName, backupFileName);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Backup oluşturulamadı: {DatabaseName}", databaseName);
                return false;
            }
        }

        /// <summary>
        /// Backup'tan geri yükler
        /// </summary>
        public async Task<bool> RestoreBackupAsync(string databaseName, string backupFileName, CancellationToken cancellationToken = default)
        {
            try
            {
                var backupDir = _applicationPaths.GetTenantBackupFolderPath();
                var backupPath = Path.Combine(backupDir, backupFileName);

                if (!File.Exists(backupPath))
                {
                    _logger?.LogWarning("Backup dosyası bulunamadı: {BackupFileName}", backupFileName);
                    return false;
                }

                var targetPath = _applicationPaths.GetTenantDatabaseFilePath(databaseName);

                // Hedef dosya varsa yedek al (opsiyonel)
                if (File.Exists(targetPath))
                {
                    var tempBackup = $"{databaseName}_before_restore_{DateTime.Now:yyyyMMdd_HHmmss}.temp";
                    await SafeFileCopyAsync(targetPath, Path.Combine(backupDir, tempBackup), cancellationToken);
                }

                await SafeFileCopyAsync(backupPath, targetPath, cancellationToken);

                _logger?.LogInformation("Backup geri yüklendi: {BackupFileName} -> {DatabaseName}", backupFileName, databaseName);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Backup geri yüklenemedi: {DatabaseName}", databaseName);
                return false;
            }
        }

        /// <summary>
        /// Eski backup'ları temizler
        /// </summary>
        public Task<int> CleanOldBackupsAsync(string databaseName, int keepLast = 10)
        {
            try
            {
                var backupDir = _applicationPaths.GetTenantBackupFolderPath();
                if (!Directory.Exists(backupDir))
                    return Task.FromResult(0);

                var backups = Directory.GetFiles(backupDir, $"{databaseName}_*.backup")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();

                var toDelete = backups.Skip(keepLast).ToList();

                foreach (var file in toDelete)
                {
                    try
                    {
                        file.Delete();
                        _logger?.LogDebug("Eski backup silindi: {FileName}", file.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Backup silinemedi: {FileName}", file.Name);
                    }
                }

                return Task.FromResult(toDelete.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Eski backup'lar temizlenemedi: {DatabaseName}", databaseName);
                return Task.FromResult(0);
            }
        }

        /// <summary>
        /// Mevcut backup'ları listeler
        /// </summary>
        public Task<List<BackupFileInfo>> GetBackupsAsync(string databaseName)
        {
            try
            {
                var backupList = new List<BackupFileInfo>();
                var backupDir = _applicationPaths.GetTenantBackupFolderPath();

                if (!Directory.Exists(backupDir))
                    return Task.FromResult(backupList);

                var backupFiles = Directory.GetFiles(backupDir, $"{databaseName}_*.backup")
                    .Select(filePath => new FileInfo(filePath))
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();

                foreach (var fileInfo in backupFiles)
                {
                    var backupType = DetermineBackupType(fileInfo.Name);

                    var backup = new BackupFileInfo
                    {
                        FileName = fileInfo.Name,
                        FilePath = fileInfo.FullName,
                        FileSizeBytes = fileInfo.Length,
                        CreatedDate = fileInfo.CreationTime,
                        BackupType = backupType,
                        DatabaseName = databaseName
                    };

                    backupList.Add(backup);
                }

                return Task.FromResult(backupList);
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "GetBackupsAsync failed: {DatabaseName}", databaseName);
                return Task.FromResult(new List<BackupFileInfo>());
            }
        }

        /// <summary>
        //Backup dosyasını doğrular
        /// </summary>
        public Task<bool> IsValidBackupFileAsync(string backupFileName)
        {
            try
            {
                var backupPath = Path.Combine(_applicationPaths.GetTenantBackupFolderPath(), backupFileName);

                if (!File.Exists(backupPath))
                    return Task.FromResult(false);

                var fileInfo = new FileInfo(backupPath);

                // Basit kontroller
                return Task.FromResult(
                    fileInfo.Length > 1024 && // 1KB'den büyük
                    fileInfo.Extension.Equals(".backup", StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public DateTime? GetLastBackupDate(string databaseName)
        {
            try
            {

                var backupDir = _applicationPaths.GetTenantBackupFolderPath();
                if (!Directory.Exists(backupDir))
                    return null;

                var lastBackup = Directory.GetFiles(backupDir, $"{databaseName}_*.backup")
                    .Select(filePath => new FileInfo(filePath))
                    .OrderByDescending(f => f.CreationTime)
                    .FirstOrDefault();
               

                return lastBackup?.CreationTime;
            }
            catch
            {
                return null;
            }
        }
       
        private async Task SafeFileCopyAsync(string source, string dest, CancellationToken cancellationToken)
        {
            // SQLite bağlantılarını temizle
            SqliteConnection.ClearAllPools();

            // Kısa bekleme
            await Task.Delay(100, cancellationToken);

            // Dosyayı kopyala
            await using var sourceStream = new FileStream(
                source, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);

            await using var destStream = new FileStream(
                dest, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);

            await sourceStream.CopyToAsync(destStream, cancellationToken);
        }
        private string GenerateBackupFileName(string databaseName)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var guid = Guid.NewGuid().ToString("N").Substring(0, 4); // 4 karakter yeterli
            return $"{databaseName}_{timestamp}_{guid}.backup";
        }
        private BackupType DetermineBackupType(string fileName)
        {
            // Dosya adı pattern'lerine göre backup tipini belirle
            if (string.IsNullOrEmpty(fileName))
                return BackupType.Manual;

            // Küçük/büyük harf duyarsız kontrol
            var lowerFileName = fileName.ToLowerInvariant();

            if (lowerFileName.Contains("before_restore") ||
                lowerFileName.Contains("_safety_") ||
                lowerFileName.Contains("_security_"))
                return BackupType.Safety;

            if (lowerFileName.Contains("auto_") ||
                lowerFileName.Contains("_auto_") ||
                lowerFileName.Contains("_scheduled_") ||
                lowerFileName.Contains("_cron_"))
                return BackupType.Automatic;

            if (lowerFileName.Contains("migration_") ||
                lowerFileName.Contains("_mig_") ||
                lowerFileName.Contains("_upgrade_"))
                return BackupType.Migration;

            if (lowerFileName.Contains("system_") ||
                lowerFileName.Contains("_sys_") ||
                lowerFileName.Contains("_internal_"))
                return BackupType.System;

            // Default: Manuel backup
            return BackupType.Manual;
        }
    }
}