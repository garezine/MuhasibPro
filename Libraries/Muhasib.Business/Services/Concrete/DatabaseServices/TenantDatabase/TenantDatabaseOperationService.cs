using Microsoft.Extensions.Logging;
using Muhasib.Business.Infrastructure.Extensions;
using Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase;
using Muhasib.Business.Services.Contracts.LogServices;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantManager;
using Muhasib.Data.Managers.DatabaseManager.Models;
using Muhasib.Data.Utilities.Responses;

namespace Muhasib.Business.Services.Concrete.DatabaseServices.TenantDatabase
{
    public class TenantDatabaseOperationService : ITenantDatabaseOperationService
    {
        private readonly ITenantSelectionService _tenantSelectionService;
        private readonly IAppSqlDatabaseManager _sqlDatabaseManager;
        private readonly ITenantMigrationManager _migrationManager;
        private readonly ILogService _logService;
        private readonly ILogger<TenantDatabaseOperationService> _logger;


        public TenantDatabaseOperationService(
            IAppSqlDatabaseManager sqlDatabaseManager,
            ITenantMigrationManager migrationManager,
            ILogService logService,
            ILogger<TenantDatabaseOperationService> logger,
            ITenantSelectionService tenantSelectionService)
        {
            _sqlDatabaseManager = sqlDatabaseManager;
            _migrationManager = migrationManager;
            _logService = logService;
            _logger = logger;
            _tenantSelectionService = tenantSelectionService;
        }

        public async Task<ApiDataResponse<bool>> RunMigrationsAsync(long maliDonemId)
        {
            try
            {
                _logger.LogInformation("Running migrations for MaliDonemId: {MaliDonemId}", maliDonemId);

                var result = await _migrationManager.RunMigrationsAsync(maliDonemId);
                if (!result)
                {
                    return new ErrorApiDataResponse<bool>(false, "Migration çalıştırılamadı");
                }

                await _logService.SistemLogService.SistemLogInformation(
                    nameof(TenantDatabaseOperationService),
                    nameof(RunMigrationsAsync),
                    $"Migration başarıyla tamamlandı. MaliDonemId: {maliDonemId}", string.Empty);

                return new SuccessApiDataResponse<bool>(true, "Migration başarıyla tamamlandı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Migration failed for MaliDonemId: {MaliDonemId}", maliDonemId);
                await _logService.SistemLogService.SistemLogException(
                    nameof(TenantDatabaseOperationService),
                    nameof(RunMigrationsAsync),
                    ex);

                return new ErrorApiDataResponse<bool>(false, $"Migration hatası: {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<bool>> CreateBackupAsync(long maliDonemId)
        {
            try
            {
                _logger.LogInformation("Creating backup for MaliDonemId: {MaliDonemId}", maliDonemId);

                // Database adını al
                var dbName = await GetDatabaseNameAsync(maliDonemId);
                if (string.IsNullOrEmpty(dbName))
                {
                    return new ErrorApiDataResponse<bool>(false, "Database adı bulunamadı");
                }

                var result = await _sqlDatabaseManager.CreateManualBackupAsync(dbName);
                if (!result)
                {
                    return new ErrorApiDataResponse<bool>(false, "Backup oluşturulamadı");
                }

                await _logService.SistemLogService.SistemLogInformation(
                    nameof(TenantDatabaseOperationService),
                    nameof(CreateBackupAsync),
                    $"Backup başarıyla oluşturuldu. MaliDonemId: {maliDonemId}", string.Empty);

                return new SuccessApiDataResponse<bool>(true, "Backup başarıyla oluşturuldu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Backup creation failed for MaliDonemId: {MaliDonemId}", maliDonemId);
                await _logService.SistemLogService.SistemLogException(
                    nameof(TenantDatabaseOperationService),
                    nameof(CreateBackupAsync),
                    ex);

                return new ErrorApiDataResponse<bool>(false, $"Backup hatası: {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<DatabaseHealthInfo>> GetHealthStatusAsync(long maliDonemId)
        {
            try
            {
                var dbName = await GetDatabaseNameAsync(maliDonemId);
                if (string.IsNullOrEmpty(dbName))
                {
                    return new ErrorApiDataResponse<DatabaseHealthInfo>(null, "Database adı bulunamadı");
                }

                var healthInfo = await _sqlDatabaseManager.GetHealthInfoAsync(dbName);
                return new SuccessApiDataResponse<DatabaseHealthInfo>(healthInfo, "Sağlık durumu alındı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed for MaliDonemId: {MaliDonemId}", maliDonemId);
                return new ErrorApiDataResponse<DatabaseHealthInfo>(null, ex.Message);
            }
        }

        public async Task<ApiDataResponse<List<BackupFileInfo>>> GetBackupHistoryAsync(long maliDonemId)
        {
            try
            {
                var dbName = await GetDatabaseNameAsync(maliDonemId);
                if (string.IsNullOrEmpty(dbName))
                {
                    return new ErrorApiDataResponse<List<BackupFileInfo>>(null, "Database adı bulunamadı");
                }

                var backups = await _sqlDatabaseManager.GetBackupHistoryAsync(dbName);
                return new SuccessApiDataResponse<List<BackupFileInfo>>(backups, "Backup geçmişi alındı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get backup history failed for MaliDonemId: {MaliDonemId}", maliDonemId);
                return new ErrorApiDataResponse<List<BackupFileInfo>>(null, ex.Message);
            }
        }

        public async Task<ApiDataResponse<bool>> PrepareDatabaseAsync(long maliDonemId)
        {
            try
            {
                _logger.LogInformation("Preparing database for MaliDonemId: {MaliDonemId}", maliDonemId);

                var result = await _migrationManager.PrepareDatabaseAsync(maliDonemId);
                if (!result)
                {
                    return new ErrorApiDataResponse<bool>(false, "Veritabanı hazırlanamadı");
                }

                return new SuccessApiDataResponse<bool>(true, "Veritabanı başarıyla hazırlandı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database preparation failed for MaliDonemId: {MaliDonemId}", maliDonemId);
                return new ErrorApiDataResponse<bool>(false, $"Hazırlama hatası: {ex.Message}");
            }
        }
        public async Task<ApiDataResponse<bool>> RestoreBackupAsync(long maliDonemId, string backupFilePath)
        {
            try
            {
                _logger.LogInformation("Restoring backup for MaliDonemId: {MaliDonemId} from path: {Path}", maliDonemId, backupFilePath);

                // 1. Database adını al
                var dbName = await GetDatabaseNameAsync(maliDonemId);
                if (string.IsNullOrEmpty(dbName))
                {
                    return new ErrorApiDataResponse<bool>(false, "Database adı bulunamadı");
                }

                // 2. Restore işlemini Manager'a delege et
                var result = await _sqlDatabaseManager.RestoreDatabaseAsync(dbName, backupFilePath);

                if (!result)
                {
                    return new ErrorApiDataResponse<bool>(false, "Backup geri yüklenemedi");
                }

                // 3. Başarılı loglama
                await _logService.SistemLogService.SistemLogInformation(
                    nameof(TenantDatabaseOperationService),
                    nameof(RestoreBackupAsync),
                    $"Backup başarıyla geri yüklendi. MaliDonemId: {maliDonemId}, Dosya: {backupFilePath}", string.Empty);

                return new SuccessApiDataResponse<bool>(true, "Backup başarıyla geri yüklendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Backup restoration failed for MaliDonemId: {MaliDonemId}", maliDonemId);
                await _logService.SistemLogService.SistemLogException(
                    nameof(TenantDatabaseOperationService),
                    nameof(RestoreBackupAsync),
                    ex);

                return new ErrorApiDataResponse<bool>(false, $"Backup geri yükleme hatası: {ex.Message}");
            }
        }
        private async Task<string> GetDatabaseNameAsync(long maliDonemId)
        {
            // Tenant bilgisi SistemDbContext'te tutulduğu için bu repo kullanılır.
            var maliDonemDb = await _tenantSelectionService.GetTenantDetailsAsync(maliDonemId);

            if (maliDonemDb == null || string.IsNullOrWhiteSpace(maliDonemDb.Data.DatabaseName))
            {
                _logger.LogWarning("MaliDonemDb kaydı veya DBName bulunamadı. MaliDonemId: {MaliDonemId}", maliDonemId);
                return string.Empty; // Veya null döndürülmeli, mevcut kullanım string.IsNullOrEmpty'ye bakıyor.
            }

            return maliDonemDb.Data.DatabaseName;
        }
    }
}
