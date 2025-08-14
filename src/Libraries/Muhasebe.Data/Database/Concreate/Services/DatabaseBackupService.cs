using Microsoft.EntityFrameworkCore; // Sistem DB'ye erişim için
using Microsoft.Extensions.Logging;
using Muhasebe.Data.Database.Helpers;
using Muhasebe.Data.Database.Interfaces.Configurations;
using Muhasebe.Data.Database.Interfaces.Operations;
using Muhasebe.Data.Database.Interfaces.Services;
using Muhasebe.Data.DataContext;
using Muhasebe.Domain.Entities.Sistem;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Data.Database.Concreate.Services
{
    // DatabaseBackupService: Constructor'dan DatabaseType kaldırıldı.
    // İşlem yapılacak veritabanının tipi dinamik olarak belirleniyor.
    public class DatabaseBackupService : IDatabaseBackupService
    {
        private readonly AppSistemDbContext _sistemContext;
        private readonly IDatabaseConfiguration _dbConfig;
        //private readonly IDatabaseBackupOperations _backupOperations; // Factory kullanılmalı
        private readonly Func<DatabaseType, IDatabaseBackupOperations> _backupOperationsFactory;
        private readonly ILogger<DatabaseBackupService> _logger;
        //private readonly DatabaseType _databaseType; // Kaldırıldı

        // Eksik: IDatabaseBackupOperations Interface ve Implementasyonları
        // public interface IDatabaseBackupOperations { Task<BackupResult> BackupDatabaseAsync(string connectionString, string dbName, string dbPath, string backupFilePath); }
        // public class SqlServerBackupOperations : IDatabaseBackupOperations { /* ... BACKUP DATABASE logic ... */ }
        // public class SQLiteBackupOperations : IDatabaseBackupOperations { /* ... File Copy logic (belki WAL checkpoint ile) ... */ }

        // Eksik: BackupResult sınıfı
        // public class BackupResult { public bool Success { get; } public string Message { get; } public string BackupFilePath { get; } public BackupResult(bool success, string message, string backupFilePath = null){...}}


        public DatabaseBackupService(
            AppSistemDbContext sistemContext,
            IDatabaseConfiguration dbConfig,
            Func<DatabaseType, IDatabaseBackupOperations> backupOperationsFactory, // DI ile factory
            ILogger<DatabaseBackupService> logger)
        {
            _sistemContext = sistemContext;
            _dbConfig = dbConfig;
            _backupOperationsFactory = backupOperationsFactory;
            _logger = logger;
            //_databaseType = databaseType; // Kaldırıldı
        }

        public async Task<BackupResult> BackupDatabaseAsync(long firmaId, long donemId, string backupBaseDirectory)
        {
            _logger.LogInformation("Starting backup for FirmaId: {FirmaId}, DonemId: {DonemId} to directory: {BackupDir}", firmaId, donemId, backupBaseDirectory);

            CalismaDonemSec donemDb = null;
            IDatabaseBackupOperations backupOps = null;
            DatabaseType dbType;

            try
            {
                // 1. Sistem DB'sinden ilgili CalismaDonemDb kaydını bul ve tipini öğren
                donemDb = await _sistemContext.CalismaDonemDbler
                    .AsNoTracking() // Sadece okuma
                    .FirstOrDefaultAsync(db => db.FirmaId == firmaId && db.CalismaDonemId == donemId)
                    .ConfigureAwait(false) ??
                    throw new InvalidOperationException($"Yedeklenecek veritabanı kaydı bulunamadı: FirmaId={firmaId}, DonemId={donemId}");

                dbType = donemDb.DatabaseType;
                backupOps = _backupOperationsFactory(dbType); // Doğru operasyon implementasyonunu al

                // 2. Yedeklenecek veritabanı için bağlantı bilgilerini al (gerekliyse)
                await _dbConfig.InitializeAsync(firmaId, donemId).ConfigureAwait(false);
                var connectionString = _dbConfig.GetConnectionString();
                var databaseName = donemDb.DBName;
                var databasePath = donemDb.DBPath; // SQLite için kaynak dosya yolu

                // 3. Yedek dosyasının adını ve tam yolunu belirle
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                // Dosya uzantısını tipe göre belirle
                var backupExtension = dbType == DatabaseType.SQLite ? ".sqlite" : ".bak"; // veya .bak her ikisi için?
                var backupFileName = $"{databaseName}_{timestamp}{backupExtension}";
                var backupFilePath = Path.Combine(backupBaseDirectory, backupFileName);

                // Yedek dizininin var olduğundan emin ol
                try
                {
                    if (!Directory.Exists(backupBaseDirectory))
                    {
                        _logger.LogInformation("Backup directory does not exist. Creating: {BackupDir}", backupBaseDirectory);
                        Directory.CreateDirectory(backupBaseDirectory);
                    }
                }
                catch (Exception dirEx)
                {
                    _logger.LogError(dirEx, "Failed to create backup directory: {BackupDir}", backupBaseDirectory);
                    throw new IOException($"Yedekleme dizini oluşturulamadı: {backupBaseDirectory}", dirEx);
                }

                _logger.LogDebug("Backup target file path: {BackupFilePath}", backupFilePath);

                // 4. Yedekleme operasyonunu çağır
                _logger.LogInformation("Executing backup operation for DB: {DbName}, Type: {DbType}", databaseName, dbType);
                var backupResult = await backupOps.BackupDatabaseAsync(
                    connectionString, // SQL Server BACKUP için gerekli
                    databaseName,     // SQL Server BACKUP için gerekli
                    databasePath,     // SQLite kaynak dosya yolu
                    backupFilePath    // Hedef yedek dosyası
                ).ConfigureAwait(false);

                if (!backupResult.Success)
                {
                    _logger.LogError("Database backup failed for {DbName}. Error: {ErrorMessage}", databaseName, backupResult.Message);
                    // Başarısız yedek dosyasını silmek iyi olabilir
                    try { if (File.Exists(backupFilePath)) File.Delete(backupFilePath); }
                    catch (Exception delEx) { _logger.LogWarning(delEx, "Failed to delete unsuccessful backup file: {BackupFilePath}", backupFilePath); }
                }
                else
                {
                    _logger.LogInformation("Database backup completed successfully for {DbName}. Backup file: {BackupFilePath}", databaseName, backupResult.BackupFilePath);
                }

                return backupResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database backup process for FirmaId: {FirmaId}, DonemId: {DonemId}. Error: {ErrorMessage}", firmaId, donemId, ex.Message);
                return new BackupResult(false, $"Yedekleme işlemi sırasında beklenmedik bir hata oluştu: {ex.Message}");
            }
            finally
            {
                // Her durumda state'i temizle
                _dbConfig.ResetState();
                _logger.LogDebug("Database configuration state reset after backup attempt.");
            }
        }
    }
}
