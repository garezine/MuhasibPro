using Microsoft.Extensions.Logging;
using Muhasebe.Data.DatabaseManager.AppDatabase;
using MuhasibPro.Contracts.SistemServices.DatabaseServices;

namespace MuhasibPro.Services.SistemServices.DatabaseServices
{
    public class AppDatabaseService : IAppDatabaseService
    {
        private readonly IAppDatabaseManager _databaseManager;
        private readonly ILogger<AppDatabaseService> _logger;

        public AppDatabaseService(
            IAppDatabaseManager databaseManager,
            ILogger<AppDatabaseService> logger)
        {
            _databaseManager = databaseManager;
            _logger = logger;
        }

        public async Task<bool> InitializeFirmaDatabaseAsync(string firmaKodu, int maliDonemYil)
        {
            return await _databaseManager.InitializeDatabaseAsync(firmaKodu, maliDonemYil);
        }

        public async Task<bool> CreateNewFirmaDatabaseAsync(string firmaKodu, int maliDonemYil)
        {
            return await _databaseManager.CreateNewDatabaseAsync(firmaKodu, maliDonemYil);
        }

        public async Task<string> GetCurrentMuhasebeVersionAsync(string firmaKodu, int maliDonemYil)
        {
            var version = await _databaseManager.GetCurrentMuhasebeVersionAsync(firmaKodu, maliDonemYil);
            return version?.MuhasebeDBVersiyon ?? "1.0.0";
        }

        public async Task<bool> UpdateMuhasebeVersionAsync(string firmaKodu, int maliDonemYil, string newVersion)
        {
            return await _databaseManager.UpdateMuhasebeVersionAsync(firmaKodu, maliDonemYil, newVersion);
        }

        public async Task<bool> IsFirmaDatabaseHealthyAsync(string firmaKodu, int maliDonemYil)
        {
            var healthInfo = await _databaseManager.GetHealthInfoAsync(firmaKodu, maliDonemYil);
            return healthInfo.CanConnect && !healthInfo.HasError;
        }

        public async Task<bool> CreateFirmaBackupAsync(string firmaKodu, int maliDonemYil)
        {
            return await _databaseManager.CreateManualBackupAsync(firmaKodu, maliDonemYil);
        }

        public async Task<string> GetFirmaHealthStatusAsync(string firmaKodu, int maliDonemYil)
        {
            var isHealthy = await IsFirmaDatabaseHealthyAsync(firmaKodu, maliDonemYil);
            var version = await GetCurrentMuhasebeVersionAsync(firmaKodu, maliDonemYil);

            return $"Firma {firmaKodu}_{maliDonemYil}: {(isHealthy ? "Healthy" : "Unhealthy")}, Version: {version}";
        }

        public async Task<List<string>> GetFirmalarNeedingUpdateAsync()
        {
            // İleride implement edilecek - şimdilik placeholder
            _logger.LogInformation("GetFirmalarNeedingUpdateAsync called - not implemented yet");
            return await Task.FromResult(new List<string>());
        }

        public async Task<bool> UpdateAllFirmaDatabasesAsync()
        {
            // İleride implement edilecek - şimdilik placeholder
            _logger.LogInformation("UpdateAllFirmaDatabasesAsync called - not implemented yet");
            return await Task.FromResult(true);
        }
    }
}
