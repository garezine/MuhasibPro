using Microsoft.Extensions.Logging;
using Muhasib.Business.Infrastructure.Extensions;
using Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase;
using Muhasib.Business.Services.Contracts.LogServices;
using Muhasib.Data.Contracts.SistemRepositories;
using Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantManager;
using Muhasib.Data.Utilities.Responses;

namespace Muhasib.Business.Services.Concrete.DatabaseServices.TenantDatabase
{
    public class TenantDatabaseLifecycleService : ITenantDatabaseLifecycleService
    {
        private readonly IAppSqlDatabaseManager _sqlDatabaseManager;
        private readonly IFirmaRepository _firmaRepository;
        private readonly ILogService _logService;
        private readonly ILogger<TenantDatabaseLifecycleService> _logger;
        private readonly IDatabaseNamingService _databaseNamingService;

        public TenantDatabaseLifecycleService(
            IAppSqlDatabaseManager sqlDatabaseManager,
            IFirmaRepository firmaRepository,
            ILogService logService,
            ILogger<TenantDatabaseLifecycleService> logger,
            IDatabaseNamingService databaseNamingService)
        {
            _sqlDatabaseManager = sqlDatabaseManager;
            _firmaRepository = firmaRepository;
            _logService = logService;
            _logger = logger;
            _databaseNamingService = databaseNamingService;
        }

        public async Task<ApiDataResponse<string>> CreateDatabaseAsync(string databaseName)
        {
            try
            {
                _logger.LogInformation("Creating tenant database: {DatabaseName}", databaseName);

                var exists = await DatabaseExistsAsync(databaseName);
                if (exists.Data)
                {
                    return new ErrorApiDataResponse<string>(
                        databaseName,
                        $"Veritabanı zaten mevcut: {databaseName}");
                }

                var created = await _sqlDatabaseManager.CreateNewDatabaseAsync(databaseName);
                if (!created)
                {
                    throw new Exception($"Veritabanı oluşturulamadı: {databaseName}");
                }

                await _logService.SistemLogService.SistemLogInformation(
                    nameof(TenantDatabaseLifecycleService),
                    nameof(CreateDatabaseAsync),
                    $"Tenant veritabanı başarıyla oluşturuldu: {databaseName}", string.Empty);

                return new SuccessApiDataResponse<string>(
                    databaseName,
                    "Veritabanı başarıyla oluşturuldu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database creation failed: {DatabaseName}", databaseName);
                await _logService.SistemLogService.SistemLogException(
                    nameof(TenantDatabaseLifecycleService),
                    nameof(CreateDatabaseAsync),
                    ex);

                return new ErrorApiDataResponse<string>(null, $"Veritabanı oluşturma hatası: {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<bool>> DeleteDatabaseAsync(string databaseName)
        {
            try
            {
                _logger.LogInformation("Deleting tenant database: {DatabaseName}", databaseName);

                var deleted = await _sqlDatabaseManager.DeleteDatabaseAsync(databaseName);
                if (!deleted)
                {
                    return new ErrorApiDataResponse<bool>(false, "Veritabanı silinemedi");
                }

                await _logService.SistemLogService.SistemLogInformation(
                    nameof(TenantDatabaseLifecycleService),
                    nameof(DeleteDatabaseAsync),
                    $"Tenant veritabanı başarıyla silindi: {databaseName}", string.Empty);

                return new SuccessApiDataResponse<bool>(true, "Veritabanı başarıyla silindi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database deletion failed: {DatabaseName}", databaseName);
                await _logService.SistemLogService.SistemLogException(
                    nameof(TenantDatabaseLifecycleService),
                    nameof(DeleteDatabaseAsync),
                    ex);

                return new ErrorApiDataResponse<bool>(false, $"Veritabanı silme hatası: {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<bool>> DatabaseExistsAsync(string databaseName)
        {
            try
            {
                var healthInfo = await _sqlDatabaseManager.GetHealthInfoAsync(databaseName);
                return await Task.FromResult(new SuccessApiDataResponse<bool>(healthInfo.CanConnect, "Kontrol tamamlandı"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database existence check failed: {DatabaseName}", databaseName);
                return new ErrorApiDataResponse<bool>(false, ex.Message);
            }
        }

        public ApiDataResponse<string> GenerateDatabaseNameAsync(string firmaKodu, int maliYil)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(firmaKodu))
                {
                    return new ErrorApiDataResponse<string>(null, "Firma kodu boş olamaz");
                }

                if (maliYil < 2000 || maliYil > 2100)
                {
                    return new ErrorApiDataResponse<string>(null, "Geçersiz mali yıl");
                }

                var databaseName = _databaseNamingService.GenerateDatabaseName(firmaKodu, maliYil);
                // Format: F-0001_2025


                _logger.LogDebug("Generated database name: {DatabaseName}", databaseName);
                return new SuccessApiDataResponse<string>(databaseName, "Database adı oluşturuldu");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database name generation failed");
                return new ErrorApiDataResponse<string>(null, ex.Message);
            }
        }
    }
}
