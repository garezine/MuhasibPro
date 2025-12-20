using Microsoft.Extensions.Logging;
using Muhasib.Business.Infrastructure.Extensions;
using Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase;
using Muhasib.Business.Services.Contracts.LogServices;
using Muhasib.Data.DataContext;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantDatabaseManager;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantSqliteManager;
using Muhasib.Data.Utilities.Responses;

namespace Muhasib.Business.Services.Concrete.DatabaseServices.TenantDatabase
{
    public class TenantSQLiteSelectionService : ITenantSQLiteSelectionService
    {
        private readonly ITenantSQLiteSelectionManager _selectionManager;
        private readonly ITenantSQLiteDatabaseOperationService _operationService;        
        private readonly ILogService _logService;
        private readonly ILogger<TenantSQLiteSelectionService> _logger;

        public TenantSQLiteSelectionService(
            ITenantSQLiteSelectionManager selectionManager,
            ILogService logService,
            ILogger<TenantSQLiteSelectionService> logger,
            ITenantSQLiteDatabaseOperationService operationService,
            ITenantSQLiteMigrationManager migrationManager)
        {
            _selectionManager = selectionManager;
            _logService = logService;
            _logger = logger;
            _operationService = operationService;            
        }

        public bool IsConnected
        {
            get
            {
                var currentTenant = _selectionManager.GetCurrentTenant();
                return currentTenant?.IsLoaded == true;
            }
        }

        public void ClearCurrentTenant() => _selectionManager.ClearCurrentTenant();

        public async Task<ApiDataResponse<bool>> DisconnectCurrentTenantAsync()
        {
            try
            {
                var currentTenant = GetCurrentTenant();
                if(!currentTenant.Success)
                {
                    return new SuccessApiDataResponse<bool>(true, "Zaten bağlı tenant yok");
                }
                ClearCurrentTenant();
                // TenantContext'i sıfırla (Empty state)               

                await _logService.SistemLogService
                    .SistemLogInformation(
                        nameof(TenantSQLiteSelectionService),
                        nameof(DisconnectCurrentTenantAsync),
                        "Tenant bağlantısı kesildi",
                        string.Empty);

                return new SuccessApiDataResponse<bool>(true, "Bağlantı başarıyla kesildi");
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Disconnect tenant failed");
                return new ErrorApiDataResponse<bool>(false, ex.Message);
            }
        }

        public ApiDataResponse<TenantContext> GetCurrentTenant()
        {
            try
            {
                var currentTenant = _selectionManager.GetCurrentTenant();

                if(currentTenant == null || !currentTenant.IsLoaded)
                {
                    return new ErrorApiDataResponse<TenantContext>(null, "Aktif mali dönem bulunamadı");
                }

                return new SuccessApiDataResponse<TenantContext>(currentTenant, "Aktif mali dönem bilgisi alındı");
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Get current tenant failed");
                return new ErrorApiDataResponse<TenantContext>(null, ex.Message);
            }
        }

        public async Task<ApiDataResponse<TenantContext>> SwitchTenantAsync(string databaseName)
        {
            try
            {
                var currentTenant = GetCurrentTenant();
                if(currentTenant.Success && currentTenant.Data?.DatabaseName == databaseName)
                {
                    _logger.LogInformation("Zaten bu tenant'a bağlı: {databaseName}", databaseName);
                    return currentTenant;
                }
                _logger.LogInformation("Switching to tenant: {databaseName}", databaseName);
                // 1. MaliDonemDb kaydı var mı kontrol et
                if(databaseName == null)
                {
                    return new ErrorApiDataResponse<TenantContext>(null, "Veritabanı kaydı bulunamadı");
                }
                // 2. Connection test
                var testResult = await _operationService.ValidateConnectionAsync(databaseName);
                if(!testResult.Success)
                {
                    return new ErrorApiDataResponse<TenantContext>(null, $"Bağlantı testi başarısız: {testResult}");
                }

                // 3. Tenant değiştir
                var tenantContext = await _selectionManager.SwitchToTenantAsync(databaseName);
                
                await _logService.SistemLogService
                    .SistemLogInformation(
                        nameof(TenantSQLiteSelectionService),
                        nameof(SwitchTenantAsync),
                        $"Tenant başarıyla değiştirildi. databaseName: {databaseName}, Database: {tenantContext.DatabaseName}",
                        string.Empty);

                return new SuccessApiDataResponse<TenantContext>(
                    tenantContext,
                    $"Mali dönem başarıyla değiştirildi: {databaseName}");
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Tenant switch failed for databaseName: {databaseName}", databaseName);
                await _logService.SistemLogService
                    .SistemLogException(nameof(TenantSQLiteSelectionService), nameof(SwitchTenantAsync), ex);

                return new ErrorApiDataResponse<TenantContext>(null, $"Mali dönem değiştirme hatası: {ex.Message}");
            }
        }
    }
}
