using Muhasebe.Data.Database.Helpers;

namespace Muhasebe.Data.Database.Interfaces.Services
{
    public interface IDatabaseBackupService
    {
        /// <summary>
        /// Belirtilen firma ve döneme ait veritabanını yedekler.
        /// </summary>
        /// <param name="fId">Firma ID.</param>
        /// <param name="dId">Çalışma Dönemi ID.</param>
        /// <param name="backupDirectory">Yedeklerin kaydedileceği ana dizin.</param>
        /// <returns>Yedekleme sonucu.</returns>
        Task<BackupResult> BackupDatabaseAsync(long fId, long dId, string backupDirectory);
    }
}
