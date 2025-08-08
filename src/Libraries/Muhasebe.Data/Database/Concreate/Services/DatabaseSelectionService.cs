using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasebe.Data.Database.Interfaces.Configurations;
using Muhasebe.Data.Database.Interfaces.Services;
using Muhasebe.Data.DataContext;

namespace Muhasebe.Data.Database.Concreate.Services
{
    public class DatabaseSelectionService : IDatabaseSelectionService
    {
        private readonly IDatabaseConfiguration _dbConfig;
        private readonly AppSistemDbContext _dbContext; // Kaydın varlığını kontrol etmek için
        private readonly ILogger<DatabaseSelectionService> _logger;

        public DatabaseSelectionService(
            IDatabaseConfiguration dbConfig,
            AppSistemDbContext dbContext,
            ILogger<DatabaseSelectionService> logger)
        {
            _dbConfig = dbConfig ?? throw new ArgumentNullException(nameof(dbConfig));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Belirtilen Firma ve Dönem için veritabanı bağlantısını seçer ve etkinleştirir.
        /// </summary>
        public async Task<bool> SelectDatabaseAsync(long firmaId, long donemId)
        {
            _logger.LogInformation("Database selection requested for FirmaId: {FirmaId}, DonemId: {DonemId}", firmaId, donemId);

            try
            {
                // 1. Adım: İlgili CalismaDonemDb kaydının varlığını ve geçerliliğini kontrol et.
                var donemDbRecord = await _dbContext.CalismaDonemDbler.FirstOrDefaultAsync(a => a.FirmaId == firmaId && a.CalismaDonemId == donemId);

                if (donemDbRecord == null)
                {
                    _logger.LogError("Database record not found for FirmaId: {FirmaId}, DonemId: {DonemId}. Cannot select database.", firmaId, donemId);
                    throw new KeyNotFoundException($"Seçilen Firma ({firmaId}) ve Dönem ({donemId}) için veritabanı kaydı bulunamadı.");
                }

                if (!donemDbRecord.AktifMi) // Eğer AktifMi gibi bir alan varsa kontrol edilebilir.
                {
                    _logger.LogWarning("Database record found but is inactive for FirmaId: {FirmaId}, DonemId: {DonemId}. DbName: {DbName}", firmaId, donemId, donemDbRecord.DBName);
                    // İsteğe bağlı olarak burada da hata fırlatılabilir veya false dönülebilir.
                    // throw new InvalidOperationException($"Seçilen veritabanı ({donemDbRecord.DBName}) aktif değil.");
                }

                _logger.LogDebug("Database record found: DbName: {DbName}, Type: {DbType}. Proceeding with configuration initialization.", donemDbRecord.DBName, donemDbRecord.DatabaseType);

                // 2. Adım: Merkezi veritabanı konfigürasyonunu başlat.
                // InitializeAsync zaten içinde gerekli CalismaDonemDb kaydını bulup provider ve connection string'i ayarlayacak.
                await _dbConfig.InitializeAsync(firmaId, donemId).ConfigureAwait(false);

                _logger.LogInformation("Database configuration successfully initialized for FirmaId: {FirmaId}, DonemId: {DonemId}. Selected DB Type: {DbType}", firmaId, donemId, _dbConfig.GetCurrentDbType());
                return true;
            }
            catch (KeyNotFoundException) // CalismaDonemDbService'den gelebilir
            {
                // Zaten loglandı, tekrar fırlat
                throw;
            }
            catch (InvalidOperationException ioex) // InitializeAsync'den gelebilir (örn: provider hatası)
            {
                _logger.LogError(ioex, "Failed to initialize database configuration for FirmaId: {FirmaId}, DonemId: {DonemId}.", firmaId, donemId);
                // Hatanın detayını koruyarak tekrar fırlat
                throw new InvalidOperationException($"Veritabanı konfigürasyonu başlatılamadı (Firma: {firmaId}, Dönem: {donemId}). Detay: {ioex.Message}", ioex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during database selection for FirmaId: {FirmaId}, DonemId: {DonemId}.", firmaId, donemId);
                // Genel hata mesajı ile fırlat
                throw new InvalidOperationException($"Veritabanı seçimi sırasında beklenmedik bir hata oluştu (Firma: {firmaId}, Dönem: {donemId}).", ex);
            }
        }

        /// <summary>
        /// Mevcut veritabanı seçimini sıfırlar.
        /// </summary>
        public void ResetSelection()
        {
            _logger.LogInformation("Resetting database selection.");
            try
            {
                _dbConfig.ResetState();
                _logger.LogDebug("Database configuration state reset successfully.");
            }
            catch (Exception ex)
            {
                // ResetState normalde hata fırlatmamalı ama loglamak iyi bir pratik.
                _logger.LogError(ex, "An error occurred while resetting database configuration state.");
            }
        }

        /// <summary>
        /// Şu anda bir veritabanının seçili olup olmadığını kontrol eder.
        /// </summary>
        public bool IsDatabaseSelected()
        {
            // IDatabaseConfiguration'ın state'ine bakarak karar veririz.
            // Örneğin, GetCurrentProvider null değilse seçilidir.
            try
            {
                return _dbConfig.GetCurrentProvider() != null && _dbConfig.GetCurrentDbType().HasValue;
            }
            catch (InvalidOperationException)
            {
                // GetCurrentProvider initialize edilmediğinde hata fırlatabilir, bu durumda seçili değildir.
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if database is selected.");
                return false; // Hata durumunda güvenli tarafta kalalım.
            }
        }
    }
}
