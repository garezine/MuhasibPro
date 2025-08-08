using Microsoft.Extensions.Logging;
using Muhasebe.Data.Database.Helpers;
using Muhasebe.Data.Database.Interfaces.Provider;
using Muhasebe.Data.Database.Interfaces.Services;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Data.Database.Concreate.Services
{
    public class DatabaseDeletionService : IDatabaseDeletionService
    {
        private readonly IDatabaseProviderFactory _providerFactory;
        private readonly ILogger<DatabaseDeletionService> _logger;
        // private readonly Func<DatabaseType, IDatabaseDeletionOperations> _deletionOperationsFactory; // Provider'a taşıdığımız için buna gerek kalmadı.

        public DatabaseDeletionService(
            IDatabaseProviderFactory providerFactory,
            ILogger<DatabaseDeletionService> logger)
        {
            _providerFactory = providerFactory;
            _logger = logger;
        }

        public async Task<DeletionResult> DeletePhysicalDatabaseAsync(string dbName, string dbDirectory, string dbPathOrIdentifier, DatabaseType dbType)
        {
            _logger.LogInformation("Attempting physical deletion for DB: {DbName}, Type: {DbType}, Path/Identifier: {DbPath}", dbName, dbType, dbPathOrIdentifier);

            try
            {
                var provider = _providerFactory.Create(dbType);

                // Provider'daki CleanupDatabaseAsync metodunu çağır
                await provider.CleanupDatabaseAsync(dbPathOrIdentifier, dbDirectory).ConfigureAwait(false);

                _logger.LogInformation("Physical database artifacts deleted successfully for {DbName}.", dbName);

                // Opsiyonel: Boş kalan dizini silmek (Dikkatli kullanılmalı!)
                // Eğer dizin sadece bu DB'ye aitse ve boş kaldıysa silinebilir.
                // if (!string.IsNullOrEmpty(dbDirectory) && Directory.Exists(dbDirectory))
                // {
                //     try
                //     {
                //         if (!Directory.EnumerateFileSystemEntries(dbDirectory).Any())
                //         {
                //             Directory.Delete(dbDirectory);
                //             _logger.LogInformation("Empty directory deleted: {DirectoryPath}", dbDirectory);
                //         }
                //         else {
                //              _logger.LogWarning("Directory {DirectoryPath} is not empty after DB deletion, skipping directory delete.", dbDirectory);
                //         }
                //     }
                //     catch (Exception dirEx)
                //     {
                //         _logger.LogError(dirEx, "Failed to delete directory {DirectoryPath} after DB deletion.", dbDirectory);
                //     }
                // }


                return new DeletionResult(
                    success: true,
                    message: $"Veritabanı/dosyalar ({dbName}) başarıyla silindi.",
                    dbName: dbName,
                    directoryPath: dbDirectory // Silinen dizin bilgisini döndürebiliriz
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during physical database deletion for {DbName}. Error: {ErrorMessage}", dbName, ex.Message);
                return new DeletionResult(false, $"Fiziksel silme sırasında hata oluştu ({dbName}): {ex.Message}", dbName, dbDirectory);
            }
        }
    }
}
