using Muhasebe.Data.Database.Helpers;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Data.Database.Interfaces.Services
{
    public interface IDatabaseRestoreService
    {
        /// <summary>
        /// Belirtilen yedek dosyasından veritabanını geri yükler.
        /// </summary>
        /// <param name="fId">İlişkili Firma ID (yeni db kaydı için).</param>
        /// <param name="dId">İlişkili Çalışma Dönemi ID (yeni db kaydı için).</param>
        /// <param name="backupFilePath">Geri yüklenecek yedek dosyasının yolu.</param>
        /// <returns>Geri yükleme sonucu.</returns>
        Task<RestoreResult> RestoreDatabaseAsync(long fId, long dId, string backupFilePath, DatabaseType targetDbType);
        Task CreateOrUpdateSistemDatabaseAsync();
    }
}
