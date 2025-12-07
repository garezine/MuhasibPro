using Microsoft.Extensions.Logging;
using Muhasib.Business.Infrastructure.Extensions;
using Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase;
using Muhasib.Business.Services.Contracts.LogServices;
using Muhasib.Data.DataContext;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantDatabaseManager;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantSqliteManager;
using Muhasib.Data.Managers.DatabaseManager.Models;
using Muhasib.Data.Utilities.Responses;
using Muhasib.Domain.Enum;

namespace Muhasib.Business.Services.Concrete.DatabaseServices.TenantDatabase
{
    public class TenantSQLiteConnectionService : ITenantSQLiteConnectionService
    {
        private readonly ITenantSQLiteConnectionManager _connectionManager;
        private readonly ITenantSQLiteSelectionManager _selectionManager;
        private readonly ILogService _logService;
        private readonly ILogger<TenantSQLiteConnectionService> _logger;

        public TenantSQLiteConnectionService(
            ITenantSQLiteConnectionManager connectionManager,
            ITenantSQLiteSelectionManager selectionManager,

            ILogService logService,
            ILogger<TenantSQLiteConnectionService> logger)
        {
            _connectionManager = connectionManager;
            _selectionManager = selectionManager;
            _logService = logService;
            _logger = logger;
        }

        private DatabaseType DatabaseType_Sqlite => DatabaseType.SQLite;

        public void ClearCurrentTenant() => _selectionManager.ClearCurrentTenant();
        public bool IsConnected
        {
            get
            {
                var currentTenant = _selectionManager.GetCurrentTenant();
                return currentTenant?.IsLoaded == true;
            }
        }
        public async Task<ApiDataResponse<bool>> DisconnectCurrentTenantAsync()
        {
            try
            {

                var currentTenant = GetCurrentTenant();
                if (!currentTenant.Success)
                {
                    return new SuccessApiDataResponse<bool>(true, "Zaten bağlı tenant yok");
                }                
                _selectionManager.ClearCurrentTenant();
                // TenantContext'i sıfırla (Empty state)               
                
                await _logService.SistemLogService
                    .SistemLogInformation(
                        nameof(TenantSQLiteConnectionService),
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

        public async Task<ApiDataResponse<string>> GetConnectionInfoAsync(string databaseName)
        {
            try
            {
                var connectionInfo = await _connectionManager.GetConnectionStringInfoAsync(
                    databaseName);

                if(string.IsNullOrEmpty(connectionInfo))
                {
                    return new ErrorApiDataResponse<string>(null, "Connection string bilgisi alınamadı");
                }

                return new SuccessApiDataResponse<string>(connectionInfo, "Connection string bilgisi alındı");
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Get connection info failed for MaliDonemId: {databaseName}", databaseName);
                return new ErrorApiDataResponse<string>(null, ex.Message);
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
                if (currentTenant.Success &&
                    currentTenant.Data?.DatabaseName == databaseName)
                {
                    _logger.LogInformation("Zaten bu tenant'a bağlı: {databaseName}", databaseName);
                    return currentTenant;
                }
                _logger.LogInformation("Switching to tenant: {databaseName}", databaseName);
                // 1. MaliDonemDb kaydı var mı kontrol et
                if (databaseName == null)
                {
                    return new ErrorApiDataResponse<TenantContext>(
                        null,
                        "Veritabanı kaydı bulunamadı");
                }
                // 2. Connection test
                var testResult = await _connectionManager.TestConnectionDetailedAsync(databaseName);
                if(testResult != ConnectionTestResult.Success)
                {
                    return new ErrorApiDataResponse<TenantContext>(null, $"Bağlantı testi başarısız: {testResult}");
                }

                // 3. Tenant değiştir
                var tenantContext = await _selectionManager.SwitchToTenantAsync(databaseName);

                await _logService.SistemLogService
                    .SistemLogInformation(
                        nameof(TenantSQLiteConnectionService),
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
                    .SistemLogException(nameof(TenantSQLiteConnectionService), nameof(SwitchTenantAsync), ex);

                return new ErrorApiDataResponse<TenantContext>(null, $"Mali dönem değiştirme hatası: {ex.Message}");
            }
        }


        public async Task<ApiDataResponse<string>> TestConnectionAsync(string databaseName)
        {
            try
            {
                var testResult = await _connectionManager.TestConnectionDetailedAsync(databaseName);

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
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Connection test failed for MaliDonemId: {MaliDonemId}", databaseName);
                return new ErrorApiDataResponse<string>(null, ex.Message);
            }
        }

        public async Task<ApiDataResponse<bool>> ValidateConnectionAsync(string databaseName)
        {
            try
            {
                var result = await _connectionManager.ValidateTenantAsync(databaseName);
                return new SuccessApiDataResponse<bool>(result.IsValid, result.Message);
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Connection validation failed for MaliDonemId: {databaseName}", databaseName);
                return new ErrorApiDataResponse<bool>(false, ex.Message);
            }
        }
    }
}
