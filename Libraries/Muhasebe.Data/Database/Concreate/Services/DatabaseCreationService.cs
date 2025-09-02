using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasebe.Data.Database.Interfaces.Provider;
using Muhasebe.Data.Database.Interfaces.Services;
using Muhasebe.Data.DataContext;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Data.Database.Concreate.Services
{
    // EnvanterPro.Business/Services/DatabaseManagement/DatabaseCreationService.cs
    // DatabaseCreationService: CalismaDonemDb kaydına DbType ve DbPath eklendi.
    // Hata yönetimi ve loglama iyileştirildi.
    public class DatabaseCreationService : IDatabaseCreationService
    {
        private readonly IDatabaseProviderFactory _providerFactory;
        // private readonly IAppDbContextFactory _dbContextFactory; // Şema uygulamak için context lazım, ama connection string ile de yapılabilir.
        private readonly ILogger<DatabaseCreationService> _logger;
        // IDatabaseConfiguration veya IAppDbContextFactory yerine doğrudan connection string almak daha bağımsız yapar.


        public DatabaseCreationService(
            IDatabaseProviderFactory providerFactory,
            // IAppDbContextFactory dbContextFactory, // Alternatif
            ILogger<DatabaseCreationService> logger)
        {
            _providerFactory = providerFactory;
            //_dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<bool> CreateAndApplySchemaAsync(string dbName, string baseDirectory, string dbPathOrIdentifier, DatabaseType dbType, string connectionStringForSchema)
        {
            _logger.LogInformation("Starting physical database creation for DbName: {DbName}, Type: {DbType}, Path/Identifier: {DbPath}", dbName, dbType, dbPathOrIdentifier);

            IDatabaseProvider provider = null;
            bool physicalDbCreated = false;
            bool schemaApplied = false;

            try
            {
                provider = _providerFactory.Create(dbType);

                // 1. (Opsiyonel ama önerilir) Eski kalıntıları temizle (Provider içinde yapılmalı)
                _logger.LogDebug("Attempting to cleanup any existing artifacts for {DbPathOrName}", dbPathOrIdentifier);
                await provider.CleanupDatabaseAsync(dbPathOrIdentifier, baseDirectory).ConfigureAwait(false); // Bu metod IDatabaseProvider'a eklenmeli

                // 2. Fiziksel veritabanı oluştur
                _logger.LogInformation("Creating physical database: {DbPathOrName}", dbPathOrIdentifier);
                await provider.CreateDatabaseAsync(dbPathOrIdentifier, baseDirectory).ConfigureAwait(false);
                physicalDbCreated = true;
                _logger.LogInformation("Physical database created successfully: {DbPathOrName}", dbPathOrIdentifier);

                // 3. Şema uygula
                _logger.LogInformation("Applying database schema for: {DbName}", dbName);
                schemaApplied = await ApplySchemaInternalAsync(provider, connectionStringForSchema).ConfigureAwait(false);
                if (!schemaApplied)
                {
                    throw new InvalidOperationException("Schema application failed!");
                }

                _logger.LogInformation("Database schema applied successfully for: {DbName}", dbName);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Physical database creation or schema application failed for DbName: {DbName}. Error: {ErrorMessage}", dbName, ex.Message);

                // Hata durumunda rollback/cleanup işlemleri
                if (physicalDbCreated && provider != null)
                {
                    // Fiziksel DB oluşturuldu ama şema uygulanamadıysa, DB'yi geri sil.
                    _logger.LogWarning("Attempting to cleanup created physical database due to schema application failure: {DbPathOrName}", dbPathOrIdentifier);
                    try
                    {
                        if (!schemaApplied)
                        {
                            await provider.CleanupDatabaseAsync(dbPathOrIdentifier, baseDirectory);
                        }
                    }
                    catch (Exception cleanEx)
                    {
                        _logger.LogError(cleanEx, "Failed to cleanup physical database during error handling for {DbName}.", dbName);
                        // Cleanup hatası ana hatayı maskelememeli, ama loglanmalı.
                    }
                }
                // Hatanın yukarı bildirilmesi önemli
                // return false; // Veya daha iyisi hatayı fırlatmak
                throw new InvalidOperationException($"Database creation/schema failed for {dbName}. See inner exception.", ex);
            }
        }

        // Helper: Şemayı uygular
        private async Task<bool> ApplySchemaInternalAsync(IDatabaseProvider provider, string connectionString)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                provider.ConfigureContext(optionsBuilder, connectionString);
                await using var context = new AppDbContext(optionsBuilder.Options);
                await provider.ApplySchemaAsync(context);
                return true; // Başarılı
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Schema application failed!");
                return false; // Başarısız
            }
        }
    }
}

/* // Alternatif: Ayrı metotlar
public async Task<bool> CreatePhysicalDatabaseAsync(string dbPathOrIdentifier, DatabaseType dbType) { ... }
public async Task ApplySchemaAsync(DatabaseType dbType, string connectionString) { ... }
*/