namespace Muhasib.Business.Services.Contracts.DatabaseServices.SistemDatabase
{
    public interface ISistemDatabaseService
    {
        // Version Management
        Task<string> GetCurrentAppVersionAsync();
        Task<string> GetCurrentSistemDbVersionAsync();
        Task<bool> UpdateAppVersionAsync(string newVersion);
        Task<bool> UpdateSistemDbVersionAsync(string newVersion);

        // Velopack Integration (ileride)
        Task<bool> CheckForUpdatesAsync();
        Task<bool> ApplyDatabaseUpdatesAsync();

        // System Status
        Task<bool> IsSystemHealthyAsync();
        Task<string> GetSystemStatusAsync();
    }
}
