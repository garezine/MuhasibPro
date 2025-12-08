using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Muhasib.Data.Managers.DatabaseManager.Concrete.Infrastructure;
using Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure;
using Muhasib.Data.Managers.DatabaseManager.Contracts.SistemDatabase;
using Muhasib.Data.Managers.DatabaseManager.Models;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.SistemDatabase
{
    public class SistemBackupManager : ISistemBackupManager
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly ILogger<SistemBackupManager> _logger;
        private readonly string databaseName = DatabaseConstants.SISTEM_DB_NAME;

        public SistemBackupManager(IApplicationPaths applicationPaths, ILogger<SistemBackupManager> logger)
        {
            _applicationPaths = applicationPaths;
            _logger = logger;
        }

        public Task<int> CleanOldBackupsAsync(int keepLast = 10)
        {
            try
            {
                var backupDir = _applicationPaths.GetBackupFolderPath();
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

        public void CleanupSqliteWalFilesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var dbPath = _applicationPaths.GetSistemDatabaseFilePath();

                // WAL ve SHM dosyalarını sil
                var walPath = dbPath + "-wal";
                var shmPath = dbPath + "-shm";

                if (File.Exists(walPath))
                {
                    File.Delete(walPath);

                }

                if (File.Exists(shmPath))
                {
                    File.Delete(shmPath);

                }
            }
            catch
            {

            }
        }

        public async Task<bool> CreateBackupAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var sourcePath = _applicationPaths.GetSistemDatabaseFilePath();
                if (!File.Exists(sourcePath))
                {
                    _logger?.LogWarning("Backup için kaynak dosya bulunamadı: {DatabaseName}", databaseName);
                    return false;
                }

                // ⭐ 1. ÖNCE WAL CHECKPOINT YAP
                await ExecuteWalCheckpointAsync(cancellationToken);

                // ⭐ 2. WAL dosyalarını temizle
                CleanupSqliteWalFilesAsync(cancellationToken);

                // ⭐ 3. Kısa bekle (file lock'lar temizlensin)
                await Task.Delay(50, cancellationToken);

                var backupDir = _applicationPaths.GetBackupFolderPath();
                var backupFileName = GenerateBackupFileName(databaseName);
                var backupPath = Path.Combine(backupDir, backupFileName);

                // ⭐ 4. ŞİMDİ backup al (temiz database dosyası)
                await SafeFileCopyAsync(sourcePath, backupPath, cancellationToken);

                _logger?.LogInformation(
                    "Backup oluşturuldu: {DatabaseName} -> {BackupPath}",
                    databaseName, backupFileName);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Backup oluşturulamadı: {DatabaseName}", databaseName);
                return false;
            }
        }

        public Task<List<BackupFileInfo>> GetBackupsAsync()
        {
            try
            {
                var backupList = new List<BackupFileInfo>();
                var backupDir = _applicationPaths.GetSistemDatabaseFilePath();

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

        public DateTime? GetLastBackupDate()
        {
            try
            {
                var backupDir = _applicationPaths.GetBackupFolderPath();
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

        public Task<bool> IsValidBackupFileAsync(string backupFileName)
        {
            try
            {
                var backupPath = Path.Combine(_applicationPaths.GetBackupFolderPath(), backupFileName);

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

        public async Task<bool> RestoreBackupAsync(string backupFileName, CancellationToken cancellationToken = default)
        {
            try
            {
                var backupDir = _applicationPaths.GetBackupFolderPath();
                var backupPath = Path.Combine(backupDir, backupFileName);

                if (!File.Exists(backupPath))
                {
                    _logger?.LogWarning("Backup dosyası bulunamadı: {BackupFileName}", backupFileName);
                    return false;
                }

                var targetPath = _applicationPaths.GetSistemDatabaseFilePath();

                // Hedef dosya varsa safety backup al
                if (File.Exists(targetPath))
                {
                    var tempBackup = $"{databaseName}_before_restore_{DateTime.Now:yyyyMMdd_HHmmss}.temp";
                    var tempBackupPath = Path.Combine(backupDir, tempBackup);

                    // ⭐ Mevcut DB'nin WAL checkpoint'ini yap
                    await ExecuteWalCheckpointAsync(cancellationToken);
                    await SafeFileCopyAsync(targetPath, tempBackupPath, cancellationToken);

                    _logger?.LogInformation("Safety backup alındı: {TempBackup}", tempBackup);
                }

                // Backup'ı geri yükle
                await SafeFileCopyAsync(backupPath, targetPath, cancellationToken);

                // ⭐ Geri yükleme sonrası WAL dosyalarını temizle
                CleanupSqliteWalFilesAsync(cancellationToken);

                _logger?.LogInformation(
                    "Backup geri yüklendi: {BackupFileName} -> {DatabaseName}",
                    backupFileName, databaseName);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Backup geri yüklenemedi: {DatabaseName}", databaseName);
                return false;
            }
        }

        #region Helper

        
        private async Task ExecuteWalCheckpointAsync(CancellationToken cancellationToken)
        {
            try
            {
                var dbPath = _applicationPaths.GetSistemDatabaseFilePath();
                var connectionString = $"Data Source={dbPath};Pooling=False"; // ⭐ Pooling kapalı

                await using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync(cancellationToken);

                // WAL checkpoint komutu - transaction'ları ana dosyaya yazar
                await using var command = connection.CreateCommand();
                command.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
                var result = await command.ExecuteScalarAsync(cancellationToken);

                _logger?.LogDebug("WAL checkpoint tamamlandı: {DatabaseName}, Result: {Result}",
                    databaseName, result);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "WAL checkpoint başarısız: {DatabaseName}", databaseName);
                // Kritik değil, devam et (backup hala alınabilir)
            }
        }
        private async Task SafeFileCopyAsync(string source, string dest, CancellationToken cancellationToken)
        {
            // SQLite bağlantılarını temizle
            SqliteConnection.ClearAllPools();

            // Kısa bekleme (file lock'lar temizlensin)
            await Task.Delay(100, cancellationToken);

            // Dosyayı kopyala
            await using var sourceStream = new FileStream(
                source, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 4096, useAsync: true);

            await using var destStream = new FileStream(
                dest, FileMode.Create, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true);

            await sourceStream.CopyToAsync(destStream, cancellationToken);
        }

        private string GenerateBackupFileName(string databaseName)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var guid = Guid.NewGuid().ToString("N").Substring(0, 4);
            return $"{databaseName}_{timestamp}_{guid}.backup";
        }

        private BackupType DetermineBackupType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BackupType.Manual;

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

            return BackupType.Manual;
        } 
        #endregion
    }
}

