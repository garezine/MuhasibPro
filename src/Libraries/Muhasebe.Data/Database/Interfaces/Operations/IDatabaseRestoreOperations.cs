using Muhasebe.Data.Database.Helpers;

namespace Muhasebe.Data.Database.Interfaces.Operations
{
    // Veritabanı geri yükleme işlemleri
    public interface IDatabaseRestoreOperations
    {
        /// <summary>
        /// Yedek dosyasından veritabanını geri yükler.
        /// </summary>
        /// <param name="backupFilePath">Geri yüklenecek yedek dosyasının tam yolu.</param>
        /// <param name="targetDatabaseName">Oluşturulacak/üzerine yazılacak veritabanının mantıksal adı.</param>
        /// <param name="targetDbDirectory">Veritabanı dosyalarının yerleştirileceği hedef dizin.</param>
        /// <param name="targetDbPath">Oluşturulacak/üzerine yazılacak veritabanının tam fiziksel yolu (SQLite için hedef dosya yolu).</param>
        /// <returns>Geri yükleme işleminin sonucunu içeren RestoreResult nesnesi.</returns>
        Task<RestoreResult> RestoreDatabaseAsync(string backupFilePath, string targetDatabaseName, string targetDbDirectory, string targetDbPath);
        // string connectionString parametresi kaldırıldı.
    }
}
