namespace MuhasibPro.Contracts.SistemServices.DatabaseServices
{
    public interface IAppDatabaseService
    {
        Task<bool> InitializeFirmaDatabaseAsync(string firmaKodu, int maliDonemYil);
        Task<bool> CreateNewFirmaDatabaseAsync(string firmaKodu, int maliDonemYil);

        // Version Management
        Task<string> GetCurrentMuhasebeVersionAsync(string firmaKodu, int maliDonemYil);
        Task<bool> UpdateMuhasebeVersionAsync(string firmaKodu, int maliDonemYil, string newVersion);

        // Health & Backup
        Task<bool> IsFirmaDatabaseHealthyAsync(string firmaKodu, int maliDonemYil);
        Task<bool> CreateFirmaBackupAsync(string firmaKodu, int maliDonemYil);
        Task<string> GetFirmaHealthStatusAsync(string firmaKodu, int maliDonemYil);

        // Multi-Firma Operations (ileride önemli olacak)
        Task<List<string>> GetFirmalarNeedingUpdateAsync();
        Task<bool> UpdateAllFirmaDatabasesAsync();
    }
}
