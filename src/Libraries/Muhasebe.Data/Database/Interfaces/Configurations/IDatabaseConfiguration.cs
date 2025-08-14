using Muhasebe.Data.Database.Interfaces.Provider;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Data.Database.Interfaces.Configurations
{
    // Veritabanı bağlantı bilgilerini ve tenant context'i yönetir
    public interface IDatabaseConfiguration
    {
        Task InitializeAsync(long firmaId, long donemId); // Yeni hali: dbType sistemden okunacak
        string GetConnectionString();
        IDatabaseProvider GetCurrentProvider();
        DatabaseType? GetCurrentDbType(); // Aktif tipi almak için eklendi
        void ResetState(); // Eklendi
    }

}
