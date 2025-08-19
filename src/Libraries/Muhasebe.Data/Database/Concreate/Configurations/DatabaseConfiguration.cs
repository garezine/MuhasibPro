using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasebe.Data.Database.Interfaces.Configurations;
using Muhasebe.Data.Database.Interfaces.Provider;
using Muhasebe.Data.DataContext;
using Muhasebe.Domain.Enum;
using Muhasebe.Domain.Interfaces.Database;

namespace Muhasebe.Data.Database.Concreate.Configurations
{
    // DatabaseConfiguration'a ResetState metodu eklendi.
    // InitializeAsync metodunda CalismaDonemDb'den DatabaseType alınması sağlandı (varsayım).
    public class DatabaseConfiguration : IDatabaseConfiguration
    {
        private readonly IUnitOfWork<AppSistemDbContext> _sysContext;
        private readonly IDatabaseProviderFactory _providerFactory;
        private readonly ILogger<DatabaseConfiguration> _logger; // Loglama eklemek iyi pratik
        private IDatabaseProvider _currentProvider;
        private string _connectionString;
        private DatabaseType? _currentDbType; // Hangi tipin aktif olduğunu tutalım

        // Eksik: CalismaDonemDb entity'sinde DatabaseType propertysi olmalı.
        // public class CalismaDonemDb { ... public DatabaseType DbType { get; set; } ... }

        public DatabaseConfiguration(
            IUnitOfWork<AppSistemDbContext> sysContext,
            IDatabaseProviderFactory providerFactory,
            ILogger<DatabaseConfiguration> logger) // Logger eklendi
        {
            _sysContext = sysContext;
            _providerFactory = providerFactory;
            _logger = logger;
        }

        // Bu metod çağrıldığında ilgili Firma/Donem için doğru provider ve connection string ayarlanır.
        // Artık dbType parametresine gerek yok, sistem DB'sinden okunacak.
        public async Task InitializeAsync(long fId, long dId)
        {
            _logger.LogDebug("Initializing database configuration for FirmaId: {FirmaId}, DonemId: {DonemId}", fId, dId);
            try
            {
                var donemDb = await _sysContext.Context.DonemDBSecim.FirstOrDefaultAsync(
                    x => x.FirmaId == fId && x.MaliDonemId == dId)
                    .ConfigureAwait(false);

                if (donemDb == null)
                {
                    _logger.LogError("CalismaDonemDb record not found for FirmaId: {FirmaId}, DonemId: {DonemId}", fId, dId);
                    throw new InvalidOperationException($"Database configuration not found for FirmaId={fId}, DonemId={dId}.");
                }

                // Sistem veritabanından DatabaseType'ı al
                var dbType = donemDb.DatabaseType; // Bu property CalismaDonemDb'de olmalı!

                _currentDbType = dbType; // Aktif tipi sakla
                _currentProvider = _providerFactory.Create(dbType);
                _connectionString = _currentProvider.GenerateConnectionString(donemDb.DBName, donemDb.Directory);

                _logger.LogInformation("Database configuration initialized. Provider: {ProviderType}, DBName: {DbName}", _currentDbType, donemDb.DBName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database configuration initialization for FirmaId: {FirmaId}, DonemId: {DonemId}", fId, dId);
                // Hata durumunda state'i temizle
                ResetState();
                throw; // Hatanın yukarıya bildirilmesi önemli
            }
        }

        public string GetConnectionString()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogWarning("Attempted to get connection string before initialization.");
                throw new InvalidOperationException("Database configuration has not been initialized.");
            }
            return _connectionString;
        }

        public IDatabaseProvider GetCurrentProvider()
        {
            if (_currentProvider == null)
            {
                _logger.LogWarning("Attempted to get database provider before initialization.");
                throw new InvalidOperationException("Database configuration has not been initialized.");
            }
            return _currentProvider;
        }

        public DatabaseType? GetCurrentDbType() => _currentDbType;


        // İşlem bittikten sonra veya hata durumunda state'i temizlemek için.
        public void ResetState()
        {
            _logger.LogDebug("Resetting database configuration state.");
            _currentProvider = null;
            _connectionString = null;
            _currentDbType = null;
        }
    }

}