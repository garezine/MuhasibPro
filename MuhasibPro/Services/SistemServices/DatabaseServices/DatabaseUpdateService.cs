using Microsoft.Extensions.Logging;
using MuhasibPro.ViewModels.Contracts.SistemServices.DatabaseServices;

namespace MuhasibPro.Services.SistemServices.DatabaseServices
{
    public class DatabaseUpdateService : IDatabaseUpdateService
    {
        private readonly ISistemDatabaseService _sistemService;
        private readonly IAppDatabaseService _appService;
        private readonly ILogger<DatabaseUpdateService> _logger;

        public DatabaseUpdateService(
            ISistemDatabaseService sistemService,
            IAppDatabaseService appService,
            ILogger<DatabaseUpdateService> logger)
        {
            _sistemService = sistemService;
            _appService = appService;
            _logger = logger;
        }

        public async Task<bool> InitializeAllDatabasesAsync()
        {
            try
            {
                _logger.LogInformation("Starting database initialization coordination...");

                // Önce sistem DB'si
                var sistemHealthy = await _sistemService.IsSystemHealthyAsync();
                if (!sistemHealthy)
                {
                    _logger.LogError("System database is not healthy");
                    return false;
                }

                _logger.LogInformation("All databases initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database initialization coordination failed");
                return false;
            }
        }

        public async Task<bool> CheckAllDatabaseVersionsAsync()
        {
            var systemVersion = await _sistemService.GetCurrentAppVersionAsync();
            var systemDbVersion = await _sistemService.GetCurrentSistemDbVersionAsync();

            _logger.LogInformation("Current versions - App: {AppVersion}, SystemDB: {SystemDbVersion}",
                systemVersion, systemDbVersion);

            return !string.IsNullOrEmpty(systemVersion);
        }

        public async Task<string> GetOverallSystemStatusAsync()
        {
            var systemStatus = await _sistemService.GetSystemStatusAsync();
            var needingUpdates = await GetDatabasesNeedingUpdateAsync();

            var status = $"{systemStatus}";
            if (needingUpdates.Any())
            {
                status += $", {needingUpdates.Count} databases need updates";
            }

            return status;
        }

        public async Task<bool> InitializeFirmaWithVersionCheckAsync(string firmaKodu, int maliDonemYil)
        {
            try
            {
                // Firma database'ini initialize et
                var success = await _appService.InitializeFirmaDatabaseAsync(firmaKodu, maliDonemYil);

                if (success)
                {
                    // Version sync
                    await SynchronizeVersionsAsync(firmaKodu, maliDonemYil);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize firma with version check: {FirmaKodu}_{MaliYil}",
                    firmaKodu, maliDonemYil);
                return false;
            }
        }

        public async Task<bool> SynchronizeVersionsAsync(string firmaKodu, int maliDonemYil)
        {
            // System version'ı al ve firma DB'sine senkronize et
            var systemVersion = await _sistemService.GetCurrentAppVersionAsync();
            return await _appService.UpdateMuhasebeVersionAsync(firmaKodu, maliDonemYil, systemVersion);
        }

        // Placeholder metodlar - ileride implement edilecek
        public async Task<bool> UpdateAllDatabasesAsync()
        {
            _logger.LogInformation("UpdateAllDatabasesAsync - placeholder");
            return await Task.FromResult(true);
        }

        public async Task<List<string>> GetDatabasesNeedingUpdateAsync()
        {
            return await Task.FromResult(new List<string>());
        }
    }
}

