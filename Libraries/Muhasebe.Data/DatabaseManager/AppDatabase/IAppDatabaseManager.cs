using Muhasebe.Data.DatabaseManager.Models;

namespace Muhasebe.Data.DatabaseManager.AppDatabase
{
    public interface IAppDatabaseManager
    {
        Task<bool> InitializeDatabaseAsync(string firmaKodu, string maliDonem);
        Task<bool> CreateManualBackupAsync(string firmaKodu, string maliDonem);
        Task<bool> CreateNewDatabaseAsync(string firmaKodu, string maliDonem);
        Task<DatabaseHealthInfo> GetHealthInfoAsync(string firmaKodu, string maliDonem);
        Task<List<BackupFileInfo>> GetBackupHistoryAsync(string firmaKodu, string maliDonem);
    }
}
