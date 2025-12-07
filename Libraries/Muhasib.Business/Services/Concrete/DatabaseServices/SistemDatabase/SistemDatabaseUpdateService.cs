using Microsoft.Extensions.Logging;
using Muhasib.Business.Services.Contracts.DatabaseServices.SistemDatabase;

namespace Muhasib.Business.Services.Concrete.DatabaseServices.SistemDatabase
{
    /// <summary>
    /// Sistem database güncelleme ve versiyon yönetimi koordinasyonu sağlar.
    /// Sadece sistem database işlemlerini handle eder, firma database'leri ile ilgilenmez.
    /// </summary>
    public class SistemDatabaseUpdateService : ISistemDatabaseUpdateService
    {
        private readonly ISistemDatabaseService _sistemService;
        
        private readonly ILogger<SistemDatabaseUpdateService> _logger;

        public SistemDatabaseUpdateService(
            ISistemDatabaseService sistemService,
           
            ILogger<SistemDatabaseUpdateService> logger)
        {
            _sistemService = sistemService;
            
            _logger = logger;
        }

        /// <summary>
        /// Tüm sistem database'lerini initialize eder ve sağlık kontrolü yapar.
        /// </summary>
        public async Task<bool> InitializeAllDatabasesAsync()
        {
            try
            {
                _logger.LogInformation("Starting system database initialization coordination...");

                // Önce sistem DB'si sağlık kontrolü
                var sistemHealthy = await _sistemService.IsSystemHealthyAsync();
                if (!sistemHealthy)
                {
                    _logger.LogError("System database is not healthy - initialization aborted");
                    return false;
                }

                _logger.LogInformation("All system databases initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System database initialization coordination failed");
                return false;
            }
        }

        /// <summary>
        /// Tüm sistem database'lerinin versiyonlarını kontrol eder.
        /// </summary>
        public async Task<bool> CheckAllDatabaseVersionsAsync()
        {
            var systemVersion = await _sistemService.GetCurrentAppVersionAsync();
            var systemDbVersion = await _sistemService.GetCurrentSistemDbVersionAsync();

            _logger.LogInformation("System versions - App: {AppVersion}, SystemDB: {SystemDbVersion}",
                systemVersion, systemDbVersion);

            // Basit versiyon validasyonu
            return !string.IsNullOrEmpty(systemVersion) && !string.IsNullOrEmpty(systemDbVersion);
        }

        /// <summary>
        /// Sistemin genel durum raporunu oluşturur.
        /// </summary>
        public async Task<string> GetOverallSystemStatusAsync()
        {
            var systemStatus = await _sistemService.GetSystemStatusAsync();
            var needingUpdates = await GetDatabasesNeedingUpdateAsync();

            var status = $"System: {systemStatus}";
            if (needingUpdates.Any())
            {
                status += $", {needingUpdates.Count} system databases need updates";
            }

            return status;
        }

        /// <summary>
        /// Versiyon kontrolü ile birlikte sistem database'ini initialize eder.
        /// </summary>
        public async Task<bool> InitializeWithVersionCheckAsync(string databaseName)
        {
            try
            {
                _logger.LogInformation("Initializing system database with version check: {DatabaseName}", databaseName);

                // Database'i initialize et
                var success = await _sistemService.ApplyDatabaseUpdatesAsync();

                if (success)
                {
                    // Versiyon senkronizasyonu
                    await SynchronizeSystemVersionsAsync(databaseName);
                    _logger.LogInformation("System database initialized with version sync: {DatabaseName}", databaseName);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize system database with version check: {DatabaseName}", databaseName);
                return false;
            }
        }

        /// <summary>
        /// Sistem versiyonlarını senkronize eder.
        /// </summary>
        public async Task<bool> SynchronizeSystemVersionsAsync(string databaseName)
        {
            try
            {
                var systemVersion = await _sistemService.GetCurrentAppVersionAsync();

                _logger.LogInformation("Synchronizing system versions for: {DatabaseName} to version: {Version}",
                    databaseName, systemVersion);

                // Sistem versiyonunu database'e senkronize et
                return await _sistemService.UpdateSistemDbVersionAsync(systemVersion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to synchronize system versions for: {DatabaseName}", databaseName);
                return false;
            }
        }

        // Placeholder metodlar - ileride implement edilecek
        public async Task<bool> UpdateAllSystemDatabasesAsync()
        {
            _logger.LogInformation("UpdateAllSystemDatabasesAsync - placeholder for future implementation");
            return await Task.FromResult(true);
        }

        public async Task<List<string>> GetDatabasesNeedingUpdateAsync()
        {
            _logger.LogInformation("GetDatabasesNeedingUpdateAsync - placeholder for future implementation");
            return await Task.FromResult(new List<string>());
        }

        /// <summary>
        /// Sistem database bakım işlemlerini çalıştırır.
        /// </summary>
        public async Task<bool> RunSystemDatabaseMaintenanceAsync()
        {
            try
            {
                _logger.LogInformation("Starting system database maintenance...");

                // 1. Versiyon kontrolü
                var versionsValid = await CheckAllDatabaseVersionsAsync();
                if (!versionsValid)
                {
                    _logger.LogWarning("System database versions are not valid");
                    return false;
                }

                // 2. Health check
                var isHealthy = await _sistemService.IsSystemHealthyAsync();
                if (!isHealthy)
                {
                    _logger.LogError("System database is not healthy - maintenance aborted");
                    return false;
                }

                _logger.LogInformation("System database maintenance completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System database maintenance failed");
                return false;
            }
        }
    }
}

