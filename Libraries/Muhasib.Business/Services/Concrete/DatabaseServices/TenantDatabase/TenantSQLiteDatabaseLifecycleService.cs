using Microsoft.Extensions.Logging;
using Muhasib.Business.Infrastructure.Extensions;
using Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase;
using Muhasib.Business.Services.Contracts.LogServices;
using Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantSqliteManager;
using Muhasib.Data.Utilities.Responses;
using Muhasib.Domain.Enum;

namespace Muhasib.Business.Services.Concrete.DatabaseServices.TenantDatabase
{
    public class TenantSQLiteDatabaseLifecycleService : ITenantSQLiteDatabaseLifecycleService
    {
        private readonly ILogService _logService;
        private readonly ILogger<TenantSQLiteDatabaseLifecycleService> _logger;        
        private readonly IDatabaseNamingService _databaseNamingService;
        private readonly ITenantSQLiteDatabaseManager _sqliteDatabaseManager;
        

        public TenantSQLiteDatabaseLifecycleService(
            ILogService logService,
            ILogger<TenantSQLiteDatabaseLifecycleService> logger,
            IDatabaseNamingService databaseNamingService,                       
            ITenantSQLiteDatabaseManager sqliteDatabaseManager)
        {
            _logService = logService;
            _logger = logger;
            _databaseNamingService = databaseNamingService;            
            _sqliteDatabaseManager = sqliteDatabaseManager;
        }

        public async Task<ApiDataResponse<string>> CreateDatabaseAsync(string databaseName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(databaseName))
                    return new ErrorApiDataResponse<string>(null, "Database adı boş olamaz");

                // ✅ Önce database'in var olup OLMADIĞINI kontrol et
                var existsResponse = await DatabaseExistsAsync(databaseName);
                if (existsResponse.Success && existsResponse.Data) // Data=true ise database VAR
                {
                    return new ErrorApiDataResponse<string>(
                        databaseName,
                        $"'{databaseName}' veritabanı zaten mevcut",
                        false,
                        ResultCodes.HATA_ZatenVar); // Özel result code
                }

                // ✅ Create database
                var created = await _sqliteDatabaseManager.CreateDatabaseAsync(databaseName);
                if (!created)
                    return new ErrorApiDataResponse<string>(null, "Veritabanı oluşturulamadı");

                // ✅ Double-check: Database gerçekten oluştu mu?
                var verifyResponse = await DatabaseExistsAsync(databaseName);
                if (!verifyResponse.Success || !verifyResponse.Data)
                {
                    _logger.LogWarning("Database oluşturuldu ama doğrulama başarısız: {DatabaseName}", databaseName);
                    // Kritik değil, sadece warning
                }

                await _logService.SistemLogService.SistemLogInformation(
                    nameof(TenantSQLiteDatabaseLifecycleService),
                    nameof(CreateDatabaseAsync),
                    $"Tenant veritabanı başarıyla oluşturuldu: {databaseName}",
                    databaseName);

                return new SuccessApiDataResponse<string>(
                    databaseName,
                    "Veritabanı başarıyla oluşturuldu",
                    true,
                    ResultCodes.BASARILI_Olusturuldu); // Özel result code
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database creation failed: {DatabaseName}", databaseName);
                await _logService.SistemLogService.SistemLogException(
                    nameof(TenantSQLiteDatabaseLifecycleService),
                    nameof(CreateDatabaseAsync),
                    ex);

                return new ErrorApiDataResponse<string>(
                    null,
                    $"Veritabanı oluşturma hatası: {ex.Message}",
                    false,
                    ResultCodes.HATA_Olusturulamadi);
            }
        }

        public async Task<ApiDataResponse<bool>> DatabaseExistsAsync(string databaseName)
        {
            try
            {
                bool canConnect = await _sqliteDatabaseManager.DatabaseExists(databaseName);

                return new SuccessApiDataResponse<bool>(
                    canConnect,  // ⭐ exists direkt kullan
                    canConnect ? "Database dosyası mevcut" : "Database dosyası bulunamadı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database existence check failed: {DatabaseName}", databaseName);
                return new ErrorApiDataResponse<bool>(false, ex.Message);
            }
        }

        public async Task<ApiDataResponse<bool>> DeleteDatabaseAsync(string databaseName)
        {
            try
            {
                _logger.LogInformation("Deleting tenant database: {DatabaseName}", databaseName);

                var deleted = await _sqliteDatabaseManager.DeleteDatabaseAsync(databaseName);
                if (!deleted)
                {
                    return new ErrorApiDataResponse<bool>(false, "Veritabanı silinemedi");
                }

                await _logService.SistemLogService.SistemLogInformation(
                    nameof(TenantSQLiteDatabaseLifecycleService),
                    nameof(DeleteDatabaseAsync),
                    $"Tenant veritabanı başarıyla silindi: {databaseName}", string.Empty);

                return new SuccessApiDataResponse<bool>(true, "Veritabanı başarıyla silindi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database deletion failed: {DatabaseName}", databaseName);
                await _logService.SistemLogService.SistemLogException(
                    nameof(TenantSQLiteDatabaseLifecycleService),
                    nameof(DeleteDatabaseAsync),
                    ex);

                return new ErrorApiDataResponse<bool>(false, $"Veritabanı silme hatası: {ex.Message}");
            }
        }

        public ApiDataResponse<string> GenerateDatabaseName(string firmaKodu, int maliYil)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(firmaKodu))
                {
                    return new ErrorApiDataResponse<string>(null, "Firma kodu boş olamaz");
                }

                if(maliYil < 2000 || maliYil > 2100)
                {
                    return new ErrorApiDataResponse<string>(null, "Geçersiz mali yıl");
                }

                var databaseName = _databaseNamingService.GenerateDatabaseName(firmaKodu, maliYil);
                // Format: F-0001_2025


                _logger.LogDebug("Generated database name: {DatabaseName}", databaseName);
                return new SuccessApiDataResponse<string>(databaseName, "Database adı oluşturuldu");
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Database name generation failed");
                return new ErrorApiDataResponse<string>(null, ex.Message);
            }
        }
    }
}
