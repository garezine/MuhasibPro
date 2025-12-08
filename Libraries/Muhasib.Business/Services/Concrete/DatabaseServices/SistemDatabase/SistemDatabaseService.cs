using Microsoft.Extensions.Logging;
using Muhasib.Business.Services.Contracts.DatabaseServices.SistemDatabase;
using Muhasib.Data.Managers.DatabaseManager.Concrete.Infrastructure;
using Muhasib.Data.Managers.DatabaseManager.Contracts.SistemDatabase;
using Muhasib.Data.Utilities.Responses;
using Muhasib.Domain.Enum;

namespace Muhasib.Business.Services.Concrete.DatabaseServices.SistemDatabase
{
    public class SistemDatabaseService : ISistemDatabaseService
    {
        public ISistemDatabaseOperationService SistemDatabaseOperation { get; }
        private readonly ISistemDatabaseManager _databaseManager;
        private readonly ILogger<SistemDatabaseService> _logger;
        private const string databaseName = DatabaseConstants.SISTEM_DB_NAME;
        public SistemDatabaseService(
            ISistemDatabaseOperationService operationService,
            ISistemDatabaseManager databaseManager,
            ILogger<SistemDatabaseService> logger)
        {
            SistemDatabaseOperation = operationService;
            _databaseManager = databaseManager;
            _logger = logger;
        }
        public async Task<ApiDataResponse<bool>> InitializeDatabaseAsync()
        { 
            try
            {
                // ✅ Önce database'in var olup OLMADIĞINI kontrol et
                var existsResponse = await ValidateConnectionAsync();
                if (!existsResponse.Success && !existsResponse.Data) // Data=true ise database VAR
                {
                    return new ErrorApiDataResponse<bool>(
                        existsResponse.Success,
                        $"'{databaseName}' veritabanı bulunamadı mevcut",
                        false,
                        ResultCodes.HATA_Bulunamadi); // Özel result code
                }

                // ✅ Create database
                var created = await _databaseManager.InitializeDatabaseAsync();
                if (!created)
                    return new ErrorApiDataResponse<bool>(created, "Veritabanı oluşturulamadı");

                // ✅ Double-check: Database gerçekten oluştu mu?
                var verifyResponse = await ValidateConnectionAsync();
                if (!verifyResponse.Success || !verifyResponse.Data)
                {
                    _logger.LogWarning("Database oluşturuldu ama doğrulama başarısız: {DatabaseName}", databaseName);
                    // Kritik değil, sadece warning
                }
                return new SuccessApiDataResponse<bool>(
                    verifyResponse.Success,
                    "Veritabanı başarıyla oluşturuldu",
                    true,
                    ResultCodes.BASARILI_Olusturuldu); // Özel result code
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database creation failed: {DatabaseName}", databaseName);                

                return new ErrorApiDataResponse<bool>(
                    false,
                    $"Veritabanı oluşturma hatası: {ex.Message}",
                    false,
                    ResultCodes.HATA_Olusturulamadi);
            }
        }
       
        #region Connection Service
       


        public Task<ApiDataResponse<string>> TestConnectionAsync()
            => SistemDatabaseOperation.TestConnectionAsync();


        public Task<ApiDataResponse<bool>> ValidateConnectionAsync()
            => SistemDatabaseOperation.ValidateConnectionAsync(); 
        #endregion
    }
}
