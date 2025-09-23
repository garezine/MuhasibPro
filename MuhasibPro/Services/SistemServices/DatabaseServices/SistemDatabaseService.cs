﻿using Microsoft.Extensions.Logging;
using Muhasebe.Data.DatabaseManager.SistemDatabase;
using MuhasibPro.ViewModels.Contracts.SistemServices.DatabaseServices;

namespace MuhasibPro.Services.SistemServices.DatabaseServices
{
    public class SistemDatabaseService : ISistemDatabaseService
    {
        private readonly ISistemDatabaseManager _databaseManager;
        private readonly ILogger<SistemDatabaseService> _logger;

        public SistemDatabaseService(
            ISistemDatabaseManager databaseManager,
            ILogger<SistemDatabaseService> logger)
        {
            _databaseManager = databaseManager;
            _logger = logger;
        }

        public async Task<string> GetCurrentAppVersionAsync()
        {
            var version = await _databaseManager.GetCurrentAppVersionAsync();
            return version?.UygulamaVersiyon ?? "1.0.0";
        }

        public async Task<string> GetCurrentSistemDbVersionAsync()
        {
            var version = await _databaseManager.GetCurrentSistemDbVersionAsync();
            return version?.SistemDBVersiyon ?? "1.0.0";
        }

        public async Task<bool> UpdateAppVersionAsync(string newVersion)
        {
            // İleride implement edilecek
            _logger.LogInformation("UpdateAppVersionAsync called with version: {Version}", newVersion);
            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateSistemDbVersionAsync(string newVersion)
        {
            // İleride implement edilecek
            _logger.LogInformation("UpdateSistemDbVersionAsync called with version: {Version}", newVersion);
            return await Task.FromResult(true);
        }

        public async Task<bool> CheckForUpdatesAsync()
        {
            // Velopack integration - ileride
            return await Task.FromResult(false);
        }

        public async Task<bool> ApplyDatabaseUpdatesAsync()
        {
            return await _databaseManager.InitializeDatabaseAsync();
        }

        public async Task<bool> IsSystemHealthyAsync()
        {
            return await _databaseManager.ValidateDatabaseAsync();
        }

        public async Task<string> GetSystemStatusAsync()
        {
            var isHealthy = await IsSystemHealthyAsync();
            var appVersion = await GetCurrentAppVersionAsync();
            var dbVersion = await GetCurrentSistemDbVersionAsync();

            return $"System: {(isHealthy ? "Healthy" : "Unhealthy")}, App: {appVersion}, DB: {dbVersion}";
        }
    }
}
