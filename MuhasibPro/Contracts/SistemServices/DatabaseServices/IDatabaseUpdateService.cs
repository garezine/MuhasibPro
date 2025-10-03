namespace MuhasibPro.Contracts.SistemServices.DatabaseServices
{
    public interface IDatabaseUpdateService
    {
        // Koordinasyon metodları
        Task<bool> InitializeAllDatabasesAsync();
        Task<bool> CheckAllDatabaseVersionsAsync();
        Task<bool> UpdateAllDatabasesAsync();

        // Status metodları
        Task<string> GetOverallSystemStatusAsync();
        Task<List<string>> GetDatabasesNeedingUpdateAsync();

        // Firma-specific operations
        Task<bool> InitializeFirmaWithVersionCheckAsync(string firmaKodu, int maliDonemYil);
        Task<bool> SynchronizeVersionsAsync(string firmaKodu, int maliDonemYil);
    }
}
