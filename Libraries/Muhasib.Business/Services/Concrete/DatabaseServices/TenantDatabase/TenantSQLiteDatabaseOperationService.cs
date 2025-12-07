using Microsoft.Extensions.Logging;
using Muhasib.Business.Infrastructure.Extensions;
using Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase;
using Muhasib.Business.Services.Contracts.LogServices;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantDatabaseManager;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantSqliteManager;
using Muhasib.Data.Managers.DatabaseManager.Models;
using Muhasib.Data.Utilities.Responses;
using Muhasib.Domain.Enum;

namespace Muhasib.Business.Services.Concrete.DatabaseServices.TenantDatabase
{
    public class TenantSQLiteDatabaseOperationService : ITenantSQLiteDatabaseOperationService
    {        
        private readonly ISQLiteDatabaseManager _sqliteDatabaseManager;
        private readonly ITenantSQLiteBackupManager _sqliteBackupManager;
        private readonly ITenantSQLiteMigrationManager _sqlitemigrationManager;
        private readonly ILogService _logService;
        private readonly ILogger<TenantSQLiteDatabaseOperationService> _logger;

        public TenantSQLiteDatabaseOperationService(

            ISQLiteDatabaseManager sqliteDatabaseManager,
            ILogService logService,
            ILogger<TenantSQLiteDatabaseOperationService> logger,
            ITenantSQLiteBackupManager sqliteBackupManager,
            ITenantSQLiteMigrationManager sqlitemigrationManager)
        {

            _sqliteDatabaseManager = sqliteDatabaseManager;
            _logService = logService;
            _sqliteBackupManager = sqliteBackupManager;
            _sqlitemigrationManager = sqlitemigrationManager;
            _logger = logger;
        }
        
        public async Task<ApiDataResponse<bool>> RunMigrationsAsync(string databaseName)
        {
            try
            {
                _logger.LogInformation("Running migrations for MaliDonemId: {MaliDonemId}", databaseName);

                var result = await _sqlitemigrationManager.RunMigrationsAsync(databaseName);
                if (!result)
                {
                    return new ErrorApiDataResponse<bool>(false, "Migration çalıştırılamadı");
                }

                await _logService.SistemLogService.SistemLogInformation(
                    nameof(TenantSQLiteDatabaseOperationService),
                    nameof(RunMigrationsAsync),
                    $"Migration başarıyla tamamlandı. databaseName: {databaseName}", string.Empty);

                return new SuccessApiDataResponse<bool>(true, "Migration başarıyla tamamlandı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Migration failed for databaseName: {databaseName}", databaseName);
                await _logService.SistemLogService.SistemLogException(
                    nameof(TenantSQLiteDatabaseOperationService),
                    nameof(RunMigrationsAsync),
                    ex);

                return new ErrorApiDataResponse<bool>(false, $"Migration hatası: {ex.Message}");
            }
        }
        public async Task<ApiDataResponse<bool>> CreateBackupAsync(string databaseName)
        {
            try
            {
                _logger.LogInformation("Creating backup for databaseName: {databaseName}", databaseName);

                // ✅ Database var mı kontrol et
                var healthInfo = await GetHealthStatusAsync(databaseName);
                if (!healthInfo.Success || !healthInfo.Data.CanConnect)
                {
                    return new ErrorApiDataResponse<bool>(
                        false,
                        "Database'e bağlanılamadı, backup alınamaz",
                        false,
                        ResultCodes.HATA_Olusturulamadi);
                }

                

                // ✅ Backup limit kontrolü (max 10 backup)
                var backupHistory = await GetBackupHistoryAsync(databaseName);
                if (backupHistory.Success && backupHistory.Data.Count >= 10)
                {
                    _logger.LogWarning("Backup limiti aşıldı ({Count}/10), eski backup'lar temizlenebilir",
                        backupHistory.Data.Count);
                }

                var result = await _sqliteBackupManager.CreateBackupAsync(databaseName);
                if (!result)
                {
                    return new ErrorApiDataResponse<bool>(
                        false,
                        "Backup oluşturulamadı",
                        false,
                        ResultCodes.HATA_Olusturulamadi);
                }

                // ✅ Backup sonrası doğrulama
                var updatedHistory = await GetBackupHistoryAsync(databaseName);
                if (updatedHistory.Success)
                {
                    var latestBackup = updatedHistory.Data.OrderByDescending(b => b.CreatedDate).FirstOrDefault();
                    if (latestBackup != null)
                    {
                        _logger.LogInformation("Backup oluşturuldu: {FileName} ({Size})",
                            latestBackup.FileName,
                            FormatFileSize(latestBackup.FileSizeBytes));
                    }
                }

                await _logService.SistemLogService.SistemLogInformation(
                    nameof(TenantSQLiteDatabaseOperationService),
                    nameof(CreateBackupAsync),
                    $"Backup başarıyla oluşturuldu. databaseName: {databaseName}",
                    string.Empty);

                return new SuccessApiDataResponse<bool>(
                    true,
                    "Backup başarıyla oluşturuldu",
                    true,
                    ResultCodes.BASARILI_Olusturuldu);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Backup creation failed for databaseName: {databaseName}", databaseName);
                await _logService.SistemLogService.SistemLogException(
                    nameof(TenantSQLiteDatabaseOperationService),
                    nameof(CreateBackupAsync),
                    ex);

                return new ErrorApiDataResponse<bool>(
                    false,
                    $"Backup hatası: {ex.Message}",
                    false,
                    ResultCodes.HATA_Olusturulamadi);
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        //todo: backup info doldurulacak

        public async Task<ApiDataResponse<List<BackupFileInfo>>> GetBackupHistoryAsync(string databaseName)
        {
            try
            {
                if (string.IsNullOrEmpty(databaseName))
                {
                    return new ErrorApiDataResponse<List<BackupFileInfo>>(null, "Database adı bulunamadı");
                }

                var backups = await _sqliteBackupManager.GetBackupsAsync(databaseName);

                // ⭐ TODO'yu burada tamamla:
                // BackupManager zaten BackupFileInfo dolduruyor, ekstra bir şey yapmaya gerek YOK!
                // Direkt döndür:

                return new SuccessApiDataResponse<List<BackupFileInfo>>(
                    backups ?? new List<BackupFileInfo>(),
                    backups?.Count > 0
                        ? $"{backups.Count} adet backup bulundu"
                        : "Backup bulunamadı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get backup history failed for databaseName: {databaseName}", databaseName);
                return new ErrorApiDataResponse<List<BackupFileInfo>>(null, ex.Message);
            }
        }

        public async Task<ApiDataResponse<DatabaseHealthInfo>> GetHealthStatusAsync(string databaseName)
        {
            try
            {                
                if (string.IsNullOrEmpty(databaseName))
                {
                    return new ErrorApiDataResponse<DatabaseHealthInfo>(null, "Database adı bulunamadı");
                }

                var healthInfo = await _sqliteDatabaseManager.GetHealthStatusAsync(databaseName);
                return new SuccessApiDataResponse<DatabaseHealthInfo>(healthInfo, "Sağlık durumu alındı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed for databaseName: {databaseName}", databaseName);
                return new ErrorApiDataResponse<DatabaseHealthInfo>(null, ex.Message);
            }
        }

        public async Task<ApiDataResponse<bool>> InitializeDatabaseAsync(string databaseName)
        {
            try
            {                
                if (string.IsNullOrEmpty(databaseName))
                {
                    return new ErrorApiDataResponse<bool>(false, "Database adı bulunamadı");
                }

                _logger.LogInformation("Preparing database for dbName: {dbName}", databaseName);
                var result = await _sqlitemigrationManager.InitializeDatabaseAsync(databaseName);
                if (!result)
                {
                    return new ErrorApiDataResponse<bool>(false, "Veritabanı hazırlanamadı");
                }

                return new SuccessApiDataResponse<bool>(true, "Veritabanı başarıyla hazırlandı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database preparation failed for databaseName: {databaseName}", databaseName);
                return new ErrorApiDataResponse<bool>(false, $"Hazırlama hatası: {ex.Message}");
            }
        }
        

        public async Task<ApiDataResponse<bool>> RestoreBackupAsync(string databaseName, string backupFilePath)
        {
            try
            {
                _logger.LogInformation("Restoring backup for databaseName: {databaseName} from path: {Path}", databaseName, backupFilePath);

               
                if (string.IsNullOrEmpty(databaseName))
                {
                    return new ErrorApiDataResponse<bool>(false, "Database adı bulunamadı");
                }

                // 2. Restore işlemini Manager'a delege et
                var result = await _sqliteBackupManager.RestoreBackupAsync(databaseName, backupFilePath);

                if (!result)
                {
                    return new ErrorApiDataResponse<bool>(false, "Backup geri yüklenemedi");
                }

                // 3. Başarılı loglama
                await _logService.SistemLogService.SistemLogInformation(
                    nameof(TenantSQLiteDatabaseOperationService),
                    nameof(RestoreBackupAsync),
                    $"Backup başarıyla geri yüklendi. databaseName: {databaseName}, Dosya: {backupFilePath}", string.Empty);

                return new SuccessApiDataResponse<bool>(true, "Backup başarıyla geri yüklendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Backup restoration failed for databaseName: {databaseName}", databaseName);
                await _logService.SistemLogService.SistemLogException(
                    nameof(TenantSQLiteDatabaseOperationService),
                    nameof(RestoreBackupAsync),
                    ex);

                return new ErrorApiDataResponse<bool>(false, $"Backup geri yükleme hatası: {ex.Message}");
            }
        }
        
    }
}
