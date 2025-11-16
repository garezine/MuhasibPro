using Microsoft.Extensions.Logging;
using Muhasib.Business.Infrastructure.Extensions;
using Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase;
using Muhasib.Business.Services.Contracts.LogServices;
using Muhasib.Data.Contracts.SistemRepositories;
using Muhasib.Data.DataContext;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantManager;
using Muhasib.Data.Managers.DatabaseManager.Models;
using Muhasib.Data.Utilities.Responses;

namespace Muhasib.Business.Services.Concrete.DatabaseServices.TenantDatabase
{
    public class TenantConnectionService : ITenantConnectionService
    {
        private readonly ITenantConnectionManager _connectionManager;
        private readonly ITenantSelectionManager _selectionManager;
        private readonly IMaliDonemDbRepository _maliDonemDbRepo;
        private readonly ILogService _logService;
        private readonly ILogger<TenantConnectionService> _logger;

        public TenantConnectionService(
            ITenantConnectionManager connectionManager,
            ITenantSelectionManager selectionManager,
            IMaliDonemDbRepository maliDonemDbRepo,
            ILogService logService,
            ILogger<TenantConnectionService> logger)
        {
            _connectionManager = connectionManager;
            _selectionManager = selectionManager;
            _maliDonemDbRepo = maliDonemDbRepo;
            _logService = logService;
            _logger = logger;
        }

        public async Task<ApiDataResponse<TenantContext>> SwitchTenantAsync(long maliDonemId)
        {
            try
            {
                _logger.LogInformation("Switching to tenant: {MaliDonemId}", maliDonemId);

                // 1. MaliDonemDb kaydı var mı kontrol et
                var maliDonemDb = await _maliDonemDbRepo.GetByMaliDonemDbIdAsync(maliDonemId);
                if (maliDonemDb == null)
                {
                    return new ErrorApiDataResponse<TenantContext>(
                        null,
                        "Mali dönem veritabanı kaydı bulunamadı");
                }

                // 2. Connection test
                var testResult = await _connectionManager.TestConnectionAsync(maliDonemId);
                if (testResult != ConnectionTestResult.Success)
                {
                    return new ErrorApiDataResponse<TenantContext>(
                        null,
                        $"Bağlantı testi başarısız: {testResult}");
                }

                // 3. Tenant değiştir
                var tenantContext = await _selectionManager.SwitchToTenantAsync(maliDonemId);

                await _logService.SistemLogService.SistemLogInformation(
                    nameof(TenantConnectionService),
                    nameof(SwitchTenantAsync),
                    $"Tenant başarıyla değiştirildi. MaliDonemId: {maliDonemId}, Database: {tenantContext.DatabaseName}", string.Empty);

                return new SuccessApiDataResponse<TenantContext>(
                    tenantContext,
                    $"Mali dönem başarıyla değiştirildi: {maliDonemDb.DBName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tenant switch failed for MaliDonemId: {MaliDonemId}", maliDonemId);
                await _logService.SistemLogService.SistemLogException(
                    nameof(TenantConnectionService),
                    nameof(SwitchTenantAsync),
                    ex);

                return new ErrorApiDataResponse<TenantContext>(
                    null,
                    $"Mali dönem değiştirme hatası: {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<bool>> ValidateConnectionAsync(long maliDonemId)
        {
            try
            {
                var result = await _connectionManager.ValidateTenantAsync(maliDonemId);
                return new SuccessApiDataResponse<bool>(result.IsValid, result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection validation failed for MaliDonemId: {MaliDonemId}", maliDonemId);
                return new ErrorApiDataResponse<bool>(false, ex.Message);
            }
        }

        public async Task<ApiDataResponse<string>> TestConnectionAsync(long maliDonemId)
        {
            try
            {
                var testResult = await _connectionManager.TestConnectionAsync(maliDonemId);

                var message = testResult switch
                {
                    ConnectionTestResult.Success => "Bağlantı başarılı",
                    ConnectionTestResult.SqlServerUnavailable => "SQL Server'a erişilemiyor",
                    ConnectionTestResult.DatabaseNotFound => "Veritabanı bulunamadı",
                    ConnectionTestResult.ConnectionFailed => "Bağlantı başarısız",
                    _ => "Bilinmeyen hata"
                };

                var success = testResult == ConnectionTestResult.Success;
                return success
                    ? new SuccessApiDataResponse<string>(message, message)
                    : new ErrorApiDataResponse<string>(message, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection test failed for MaliDonemId: {MaliDonemId}", maliDonemId);
                return new ErrorApiDataResponse<string>(null, ex.Message);
            }
        }

        public ApiDataResponse<TenantContext> GetCurrentTenant()
        {
            try
            {
                var currentTenant = _selectionManager.GetCurrentTenant();

                if (currentTenant == null || !currentTenant.IsLoaded)
                {
                    return new ErrorApiDataResponse<TenantContext>(
                        null,
                        "Aktif mali dönem bulunamadı");
                }

                return new SuccessApiDataResponse<TenantContext>(
                    currentTenant,
                    "Aktif mali dönem bilgisi alındı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get current tenant failed");
                return new ErrorApiDataResponse<TenantContext>(null, ex.Message);
            }
        }

        public async Task<ApiDataResponse<bool>> DisconnectCurrentTenantAsync()
        {
            try
            {
                _logger.LogInformation("Disconnecting current tenant");

                // TenantContext'i sıfırla (Empty state)
                await _selectionManager.SwitchToTenantAsync(0);

                await _logService.SistemLogService.SistemLogInformation(
                    nameof(TenantConnectionService),
                    nameof(DisconnectCurrentTenantAsync),
                    "Tenant bağlantısı kesildi", string.Empty);

                return new SuccessApiDataResponse<bool>(true, "Bağlantı başarıyla kesildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Disconnect tenant failed");
                return new ErrorApiDataResponse<bool>(false, ex.Message);
            }
        }

        public async Task<ApiDataResponse<string>> GetConnectionInfoAsync(long maliDonemId)
        {
            try
            {
                var connectionInfo = await _connectionManager.GetConnectionStringInfoAsync(maliDonemId);

                if (string.IsNullOrEmpty(connectionInfo))
                {
                    return new ErrorApiDataResponse<string>(
                        null,
                        "Connection string bilgisi alınamadı");
                }

                return new SuccessApiDataResponse<string>(
                    connectionInfo,
                    "Connection string bilgisi alındı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get connection info failed for MaliDonemId: {MaliDonemId}", maliDonemId);
                return new ErrorApiDataResponse<string>(null, ex.Message);
            }
        }
    }
}
