using Microsoft.Extensions.Logging;
using Muhasib.Business.Services.Contracts.DatabaseServices.SistemDatabase;
using Muhasib.Data.Managers.DatabaseManager.Concrete.Infrastructure;
using Muhasib.Data.Managers.DatabaseManager.Contracts.SistemDatabase;
using Muhasib.Data.Managers.DatabaseManager.Models;
using Muhasib.Data.Utilities.Responses;
using Muhasib.Domain.Enum;

namespace Muhasib.Business.Services.Concrete.DatabaseServices.SistemDatabase
{
    public class SistemDatabaseOperationService : ISistemDatabaseOperationService
    {
        private readonly ILogger<SistemDatabaseOperationService> _logger;
        private readonly ISistemDatabaseManager _sistemDatabaseManager;
        private readonly ISistemBackupManager _backupManager;
        private const string databaseName = DatabaseConstants.SISTEM_DB_NAME;

        public SistemDatabaseOperationService(
            ILogger<SistemDatabaseOperationService> logger,
            ISistemDatabaseManager sistemDatabaseManager,
            ISistemBackupManager backupManager)
        {
            _logger = logger;
            _sistemDatabaseManager = sistemDatabaseManager;
            _backupManager = backupManager;
        }

        public async Task<ApiDataResponse<DatabaseHealthInfo>> GetHealthStatusAsync()
        {
            try
            {
                var healthInfo = await _sistemDatabaseManager.GetHealthStatusAsync();
                return new SuccessApiDataResponse<DatabaseHealthInfo>(healthInfo, "Sağlık durumu alındı");
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Health check failed for databaseName: {databaseName}", databaseName);
                return new ErrorApiDataResponse<DatabaseHealthInfo>(null, ex.Message);
            }
        }
        public async Task<ApiDataResponse<bool>> ValidateConnectionAsync()
        {
            try
            {
                var result = await _sistemDatabaseManager.ValidateSistemDatabaseAsync();
                return new SuccessApiDataResponse<bool>(result.IsValid, result.Message);
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Connection validation failed for Sistem Database: {databaseName}", databaseName);
                return new ErrorApiDataResponse<bool>(false, ex.Message);
            }
        }
        public async Task<ApiDataResponse<bool>> CreateBackupAsync()
        {
            try
            {
                _logger.LogInformation("Creating backup for databaseName: {databaseName}", databaseName);

                // ✅ Database var mı kontrol et
                var healthInfo = await GetHealthStatusAsync();
                if (!healthInfo.Success || !healthInfo.Data.CanConnect)
                {
                    return new ErrorApiDataResponse<bool>(
                        false,
                        "Database'e bağlanılamadı, backup alınamaz",
                        false,
                        ResultCodes.HATA_Olusturulamadi);
                }



                // ✅ Backup limit kontrolü (max 10 backup)
                var backupHistory = await GetBackupHistoryAsync();
                if (backupHistory.Success && backupHistory.Data.Count >= 10)
                {
                    _logger.LogWarning("Backup limiti aşıldı ({Count}/10), eski backup'lar temizlenebilir",
                        backupHistory.Data.Count);
                }

                var result = await _backupManager.CreateBackupAsync();
                if (!result)
                {
                    return new ErrorApiDataResponse<bool>(
                        false,
                        "Backup oluşturulamadı",
                        false,
                        ResultCodes.HATA_Olusturulamadi);
                }

                // ✅ Backup sonrası doğrulama
                var updatedHistory = await GetBackupHistoryAsync();
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
                return new SuccessApiDataResponse<bool>(
                    true,
                    "Backup başarıyla oluşturuldu",
                    true,
                    ResultCodes.BASARILI_Olusturuldu);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Backup creation failed for databaseName: {databaseName}", databaseName);               

                return new ErrorApiDataResponse<bool>(
                    false,
                    $"Backup hatası: {ex.Message}",
                    false,
                    ResultCodes.HATA_Olusturulamadi);
            }
        }
        public async Task<ApiDataResponse<List<BackupFileInfo>>> GetBackupHistoryAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(databaseName))
                {
                    return new ErrorApiDataResponse<List<BackupFileInfo>>(null, "Database adı bulunamadı");
                }

                var backups = await _backupManager.GetBackupsAsync();

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
        public async Task<ApiDataResponse<bool>> RestoreBackupAsync(string backupFilePath)
        {
            try
            {
                _logger.LogInformation("Restoring backup for databaseName: {databaseName} from path: {Path}", databaseName, backupFilePath);
            

                // 2. Restore işlemini Manager'a delege et
                var result = await _backupManager.RestoreBackupAsync(backupFilePath);

                if (!result)
                {
                    return new ErrorApiDataResponse<bool>(false, "Backup geri yüklenemedi");
                }

                // 3. Başarılı loglama
               
                _logger.LogInformation("Backup restoration success for databaseName: {databaseName}", databaseName);
                return new SuccessApiDataResponse<bool>(true, "Backup başarıyla geri yüklendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Backup restoration failed for databaseName: {databaseName}", databaseName);              

                return new ErrorApiDataResponse<bool>(false, $"Backup geri yükleme hatası: {ex.Message}");
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
    }
}
