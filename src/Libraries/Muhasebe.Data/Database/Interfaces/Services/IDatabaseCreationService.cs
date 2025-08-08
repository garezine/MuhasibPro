using Muhasebe.Domain.Enum;

namespace Muhasebe.Data.Database.Interfaces.Services
{
    public interface IDatabaseCreationService
    {
        /// <summary>
        /// Verilen parametrelerle fiziksel veritabanını oluşturur ve şemasını uygular.
        /// </summary>
        /// <param name="dbName">Oluşturulacak veritabanının adı.</param>
        /// <param name="baseDirectory">Veritabanı dosyalarının (varsa) bulunacağı temel dizin.</param>
        /// <param name="dbPathOrIdentifier">Provider'a özel veritabanı yolu veya tanımlayıcısı (SQLite için tam path, SQL Server için DB adı).</param>
        /// <param name="dbType">Oluşturulacak veritabanı türü.</param>
        /// <param name="connectionStringForSchema">Şema uygulamak için kullanılacak bağlantı dizesi.</param>
        /// <returns>Başarılı olursa true, aksi takdirde false döner veya hata fırlatır.</returns>
        Task<bool> CreateAndApplySchemaAsync(string dbName, string baseDirectory, string dbPathOrIdentifier, DatabaseType dbType, string connectionStringForSchema);

        // Belki sadece CreateDatabaseAsync ve ApplySchemaAsync ayrı ayrı da sunulabilir.
        // Task<bool> CreatePhysicalDatabaseAsync(string dbPathOrIdentifier, DatabaseType dbType);
        // Task ApplySchemaAsync(DatabaseType dbType, string connectionString);
    }
}
