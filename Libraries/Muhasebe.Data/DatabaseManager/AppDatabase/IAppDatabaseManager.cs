using Muhasebe.Data.DatabaseManager.Models;
using Muhasebe.Domain.Entities.DegerlerEntity;

namespace Muhasebe.Data.DatabaseManager.AppDatabase
{
    public interface IAppDatabaseManager
    {
        Task<bool> InitializeDatabaseAsync(string firmaKodu, int maliDonemYil);
        Task<bool> CreateManualBackupAsync(string firmaKodu, int maliDonemYil);
        Task<bool> CreateNewDatabaseAsync(string firmaKodu, int maliDonemYil);
        Task<DatabaseHealthInfo> GetHealthInfoAsync(string firmaKodu, int maliDonemYil);
        Task<List<BackupFileInfo>> GetBackupHistoryAsync(string firmaKodu, int maliDonemYil);
        Task<MuhasebeVersiyon> GetCurrentMuhasebeVersionAsync(string firmaKodu, int maliDonemYil);
        Task<bool> UpdateMuhasebeVersionAsync(string firmaKodu, int maliDonemYil, string newVersion);
    }
}
