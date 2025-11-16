using Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.Infrastructure
{
    public class DatabaseNamingService : IDatabaseNamingService
    {
        public string GenerateDatabaseName(string firmaKodu, int maliYil)
        {
            // Örn: MHSB_FIRMAA_2025
            const string Prefix = "MHSB";
            // Tüm SQL adlarını büyük harf kullanmak ve özel karakterden kaçınmak iyi bir pratiktir.
            return $"{Prefix}_{firmaKodu.ToUpper()}_{maliYil}";
        }

        public string GenerateBackupFileName(string identifier)
        {
            return $"{identifier}_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
        }
    }
}
